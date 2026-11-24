import hmac
import os
import re
from functools import wraps
from secrets import token_urlsafe

import click
import pytds
from flask import (
    Flask, abort, flash, g, redirect, render_template, request, session, url_for,
)
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
from werkzeug.security import check_password_hash, generate_password_hash

import cuentas


NOMBRES = {
    "nombre": "Nombre",
    "primer_apellido": "Primer apellido",
    "segundo_apellido": "Segundo apellido",
}

PERMISOS = {
    "area_usuario": ("usuario", None),
    "area_personal": ("empleado", cuentas.ROL_PERSONAL),
}

HASH_FICTICIO = generate_password_hash(token_urlsafe(32))


def correo_valido(correo):
    return (
        len(correo.encode("utf-16-le")) <= 300
        and re.fullmatch(r"[^\s@]+@[^\s@]+\.[^\s@]+", correo)
    )


def validar_registro(formulario):
    datos = {
        campo: formulario.get(campo, "").strip()
        for campo in ("cedula", *NOMBRES, "correo")
    }
    errores = {}
    cedula = datos["cedula"]

    if not re.fullmatch(r"[0-9]{1,10}", cedula) or not 0 < int(cedula) <= 2147483647:
        errores["cedula"] = "Ingrese una cédula numérica válida."

    for campo, etiqueta in NOMBRES.items():
        if not datos[campo] or len(datos[campo].encode("utf-16-le")) > 160:
            errores[campo] = f"Revise el campo «{etiqueta}» (máximo 80 caracteres)."

    datos["correo"] = datos["correo"].lower()
    if not correo_valido(datos["correo"]):
        errores["correo"] = "Ingrese un correo electrónico válido."

    contrasena = formulario.get("contrasena", "")
    if not 12 <= len(contrasena) <= 128:
        errores["contrasena"] = "Use entre 12 y 128 caracteres."

    if contrasena != formulario.get("confirmacion", ""):
        errores["confirmacion"] = "Las contraseñas deben coincidir."

    if not errores:
        datos["cedula"] = int(cedula)

    return datos, errores


def requiere_permiso(permiso):
    tipo, rol = PERMISOS[permiso]

    def decorar(vista):
        @wraps(vista)
        def protegida(*args, **kwargs):
            if g.cuenta is None:
                flash("Inicie sesión para continuar.", "aviso")
                destino = "ingreso" if tipo == "usuario" else "ingreso_personal"
                return redirect(url_for(destino))

            if g.cuenta["tipo"] != tipo or (rol and rol not in g.cuenta["roles"]):
                abort(403)

            return vista(*args, **kwargs)

        return protegida

    return decorar


def create_app():
    app = Flask(__name__)
    secreto = os.environ.get("AUTH_SECRET", "")
    if len(secreto) < 32:
        raise RuntimeError("Configure AUTH_SECRET con un secreto aleatorio de al menos 32 caracteres.")

    app.config.update(
        SECRET_KEY=secreto,
        SESSION_COOKIE_NAME="fundacion_sesion",
        SESSION_COOKIE_HTTPONLY=True,
        SESSION_COOKIE_SECURE=os.getenv("AUTH_LOCAL_HTTP") != "1",
        SESSION_COOKIE_SAMESITE="Lax",
        MAX_CONTENT_LENGTH=16 * 1024,
        RATELIMIT_STORAGE_URI=os.getenv("AUTH_RATE_LIMIT_STORAGE", "memory://"),
    )
    limiter = Limiter(get_remote_address, app=app)

    @app.before_request
    def preparar_solicitud():
        g.cuenta = None
        if request.endpoint in (None, "static"):
            return

        if request.method == "POST":
            csrf = session.get("csrf")
            enviado = request.form.get("csrf", "")
            if not csrf or not hmac.compare_digest(csrf.encode(), enviado.encode()):
                abort(400)

        if "token" in session:
            g.cuenta = cuentas.cuenta_de_sesion(session["tipo"], session["token"])
            if g.cuenta is None:
                session.clear()

    @app.context_processor
    def preparar_formularios():
        def csrf_token():
            if "csrf" not in session:
                session["csrf"] = token_urlsafe(32)
            return session["csrf"]

        return {"csrf_token": csrf_token}

    @app.after_request
    def proteger_respuesta(respuesta):
        if request.endpoint != "static":
            respuesta.headers["Cache-Control"] = "no-store"

        respuesta.headers["X-Content-Type-Options"] = "nosniff"
        respuesta.headers["Content-Security-Policy"] = (
            "default-src 'self'; base-uri 'none'; frame-ancestors 'none'; form-action 'self'"
        )
        respuesta.headers["Referrer-Policy"] = "no-referrer"
        return respuesta

    def destino_de_cuenta():
        destino = "area_usuario" if g.cuenta["tipo"] == "usuario" else "area_personal"
        return url_for(destino)

    @app.get("/")
    def inicio():
        return redirect(destino_de_cuenta() if g.cuenta else url_for("ingreso"))

    @app.route("/registro", methods=["GET", "POST"])
    @limiter.limit("8 per minute", methods=["POST"])
    def registro():
        if g.cuenta:
            return redirect(destino_de_cuenta())

        datos, errores = {}, {}
        if request.method == "POST":
            datos, errores = validar_registro(request.form)
            if not errores:
                try:
                    cuentas.crear_cuenta("usuario", datos, request.form["contrasena"])
                except cuentas.CuentaDuplicada:
                    errores["general"] = "Ya existe una cuenta con esa cédula o correo."
                except cuentas.DatosPersonaDistintos:
                    errores["general"] = "Revise los datos de identificación ingresados."
                else:
                    flash("Cuenta creada. Ya puede iniciar sesión.", "exito")
                    return redirect(url_for("ingreso"), code=303)

        return render_template(
            "registro.html", datos=datos, errores=errores, nombres=NOMBRES
        ), 400 if errores else 200

    def iniciar_sesion(tipo):
        if g.cuenta:
            return redirect(destino_de_cuenta())

        correo, error = "", None
        if request.method == "POST":
            correo = request.form.get("correo", "").strip().lower()
            contrasena = request.form.get("contrasena", "")

            if not correo_valido(correo) or not 1 <= len(contrasena) <= 128:
                return render_template(
                    "ingreso.html",
                    tipo=tipo,
                    correo=correo,
                    error="Revise el correo y la contraseña.",
                ), 400

            cuenta = cuentas.buscar_cuenta(tipo, correo)
            valida = check_password_hash(cuenta[1] if cuenta else HASH_FICTICIO, contrasena)

            if cuenta and valida:
                token = token_urlsafe(32)
                cuentas.guardar_sesion(tipo, cuenta[0], token)
                session.clear()
                session.update(tipo=tipo, token=token)
                destino = "area_usuario" if tipo == "usuario" else "area_personal"
                flash("Sesión iniciada.", "exito")
                return redirect(url_for(destino), code=303)

            error = "Correo o contraseña incorrectos."

        return render_template(
            "ingreso.html", tipo=tipo, correo=correo, error=error
        ), 401 if error else 200

    @app.route("/ingreso", methods=["GET", "POST"])
    @limiter.limit("8 per minute", methods=["POST"])
    def ingreso():
        return iniciar_sesion("usuario")

    @app.route("/personal/ingreso", methods=["GET", "POST"])
    @limiter.limit("8 per minute", methods=["POST"])
    def ingreso_personal():
        return iniciar_sesion("empleado")

    @app.get("/cuenta")
    @requiere_permiso("area_usuario")
    def area_usuario():
        return render_template("cuenta.html", titulo="Mi cuenta")

    @app.get("/personal")
    @requiere_permiso("area_personal")
    def area_personal():
        return render_template("cuenta.html", titulo="Área del personal")

    @app.post("/salir")
    def salir():
        token = session.get("token")
        tipo = session.get("tipo")
        if token:
            cuentas.eliminar_sesion(token)

        session.clear()
        flash("Sesión cerrada.", "exito")
        destino = "ingreso_personal" if tipo == "empleado" else "ingreso"
        return redirect(url_for(destino), code=303)

    @app.errorhandler(400)
    @app.errorhandler(403)
    @app.errorhandler(413)
    @app.errorhandler(429)
    def mostrar_error(error):
        mensajes = {
            400: "El formulario venció. Abra la página de nuevo e inténtelo otra vez.",
            403: "Su cuenta no tiene permiso para acceder a esta página.",
            413: "El formulario supera el tamaño permitido.",
            429: "Hay demasiados intentos. Espere un minuto e inténtelo de nuevo.",
        }
        respuesta = error.get_response()
        respuesta.set_data(render_template("error.html", mensaje=mensajes[error.code]))
        return respuesta

    @app.errorhandler(pytds.Error)
    def error_bd(error):
        app.logger.error("Error de SQL Server en autenticación (%s).", getattr(error, "number", None))
        return render_template(
            "error.html", mensaje="El servicio de acceso no está disponible por ahora."
        ), 503

    @app.cli.command("crear-empleado")
    @click.option("--cedula", prompt="Cédula")
    @click.option("--nombre", prompt="Nombre")
    @click.option("--primer-apellido", prompt="Primer apellido")
    @click.option("--segundo-apellido", prompt="Segundo apellido")
    @click.option("--correo", prompt="Correo")
    @click.password_option(prompt="Contraseña", confirmation_prompt="Confirmar contraseña")
    def crear_empleado(cedula, nombre, primer_apellido, segundo_apellido, correo, password):
        datos, errores = validar_registro({
            "cedula": cedula,
            "nombre": nombre,
            "primer_apellido": primer_apellido,
            "segundo_apellido": segundo_apellido,
            "correo": correo,
            "contrasena": password,
            "confirmacion": password,
        })
        if errores:
            raise click.ClickException(" ".join(errores.values()))

        try:
            cuentas.crear_cuenta("empleado", datos, password)
        except cuentas.CuentaDuplicada:
            raise click.ClickException("Ya existe un empleado con esa cédula o correo.") from None
        except cuentas.DatosPersonaDistintos:
            raise click.ClickException("Los nombres no coinciden con la persona registrada.") from None

        click.echo("Cuenta de personal creada.")

    return app