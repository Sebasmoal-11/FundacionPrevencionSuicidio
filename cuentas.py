import os
from contextlib import contextmanager
from hashlib import sha256

import pytds
from werkzeug.security import generate_password_hash


COLUMNAS = {
    "usuario": "cedula_usuario",
    "empleado": "cedula_empleado",
}

FECHAS = {
    "usuario": "fecha_registro",
    "empleado": "fecha_contratacion",
}

ROL_PERSONAL = "Personal"


class CuentaDuplicada(Exception):
    pass


class DatosPersonaDistintos(Exception):
    pass


@contextmanager
def conexion():
    with pytds.connect(
        server=os.getenv("SQL_HOST", "localhost"),
        port=int(os.getenv("SQL_PORT", "1433")),
        database=os.getenv("SQL_DATABASE", "fundacion_prevencion"),
        user=os.environ["SQL_USER"],
        password=os.environ["SQL_PASSWORD"],
        cafile=os.getenv("SQL_CA_FILE"),
        validate_host=True,
        login_timeout=5,
        timeout=10,
    ) as db:
        with db.cursor() as cursor:
            yield cursor
        db.commit()


def crear_cuenta(tipo, datos, contrasena):
    fecha = FECHAS[tipo]
    hash_contrasena = generate_password_hash(contrasena)
    cedula = datos["cedula"]
    nombres = (
        datos["nombre"],
        datos["primer_apellido"],
        datos["segundo_apellido"],
    )

    try:
        with conexion() as db:
            db.execute(
                """SELECT nombre, primer_apellido, segundo_apellido
                   FROM dbo.persona WITH (UPDLOCK, HOLDLOCK)
                   WHERE cedula = %s""",
                (cedula,),
            )
            persona = db.fetchone()

            if persona is None:
                db.execute(
                    """INSERT dbo.persona
                       (cedula, nombre, primer_apellido, segundo_apellido)
                       VALUES (%s, %s, %s, %s)""",
                    (cedula, *nombres),
                )
            elif tuple(nombre.casefold() for nombre in persona) != tuple(
                nombre.casefold() for nombre in nombres
            ):
                raise DatosPersonaDistintos

            db.execute(
                f"""INSERT dbo.{tipo}
                       (cedula, correo, contrasena_hash, {fecha}, id_estado_activo)
                    VALUES (
                        %s, %s, %s, SYSUTCDATETIME(),
                        (SELECT id_estado_activo FROM dbo.estadoactivo
                         WHERE nombre = N'Activo')
                    )""",
                (cedula, datos["correo"], hash_contrasena),
            )

            if tipo == "empleado":
                db.execute(
                    """DECLARE @rol INT;
                       SELECT @rol = id_rol
                       FROM dbo.rol WITH (UPDLOCK, HOLDLOCK)
                       WHERE nombre = %s;

                       IF @rol IS NULL
                       BEGIN
                           SELECT @rol = COALESCE(MAX(id_rol), 0) + 1
                           FROM dbo.rol WITH (UPDLOCK, HOLDLOCK);
                           INSERT dbo.rol (id_rol, nombre, descripcion)
                           VALUES (@rol, %s, N'Acceso al área del personal');
                       END;

                       INSERT dbo.empleadorol (cedula_empleado, id_rol)
                       VALUES (%s, @rol);""",
                    (ROL_PERSONAL, ROL_PERSONAL, cedula),
                )
    except pytds.IntegrityError as error:
        if error.number in (2601, 2627):
            raise CuentaDuplicada from error
        raise


def buscar_cuenta(tipo, correo):
    if tipo not in COLUMNAS:
        raise ValueError("Tipo de cuenta inválido.")

    with conexion() as db:
        db.execute(
            f"""SELECT c.cedula, c.contrasena_hash
                FROM dbo.{tipo} c
                JOIN dbo.estadoactivo e ON e.id_estado_activo = c.id_estado_activo
                WHERE c.correo = %s AND e.nombre = N'Activo'""",
            (correo,),
        )
        return db.fetchone()


def guardar_sesion(tipo, cedula, token):
    columna = COLUMNAS[tipo]
    token_hash = sha256(token.encode()).hexdigest()

    with conexion() as db:
        db.execute(
            "DELETE FROM dbo.sesionautenticacion WHERE vence <= SYSUTCDATETIME()"
        )
        db.execute(
            f"""INSERT dbo.sesionautenticacion (token_hash, {columna}, vence)
                VALUES (%s, %s, DATEADD(hour, 8, SYSUTCDATETIME()))""",
            (token_hash, cedula),
        )


def eliminar_sesion(token):
    token_hash = sha256(token.encode()).hexdigest()

    with conexion() as db:
        db.execute(
            "DELETE FROM dbo.sesionautenticacion WHERE token_hash = %s",
            (token_hash,),
        )


def cuenta_de_sesion(tipo, token):
    columna = COLUMNAS[tipo]
    token_hash = sha256(token.encode()).hexdigest()

    with conexion() as db:
        db.execute(
            f"""SELECT c.cedula, p.nombre
                FROM dbo.sesionautenticacion s
                JOIN dbo.{tipo} c ON c.cedula = s.{columna}
                JOIN dbo.persona p ON p.cedula = c.cedula
                JOIN dbo.estadoactivo e ON e.id_estado_activo = c.id_estado_activo
                WHERE s.token_hash = %s
                  AND s.vence > SYSUTCDATETIME()
                  AND e.nombre = N'Activo'""",
            (token_hash,),
        )
        fila = db.fetchone()

        if fila is None:
            return None

        cedula, nombre = fila
        roles = set()

        if tipo == "empleado":
            db.execute(
                """SELECT r.nombre
                   FROM dbo.rol r
                   JOIN dbo.empleadorol er ON er.id_rol = r.id_rol
                   WHERE er.cedula_empleado = %s""",
                (cedula,),
            )
            roles = {rol for (rol,) in db.fetchall()}

        return {
            "tipo": tipo,
            "cedula": cedula,
            "nombre": nombre,
            "roles": roles,
        }