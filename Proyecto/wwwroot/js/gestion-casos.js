// Gestión de casos: modales y registro de un caso nuevo.
(() => {
    "use strict";

    if (window.__gestionCasosInicializado) {
        return;
    }
    window.__gestionCasosInicializado = true;

    const modalNuevo = document.getElementById("modal-nuevo");
    const form = document.getElementById("formCrearCaso");
    const formEstado = document.getElementById("formCambiarEstado");
    const estadoCasoId = document.getElementById("estadoCasoId");
    const estadoCasoSelect = document.getElementById("estadoCasoSelect");
    const btnGuardarEstado = document.getElementById("btnGuardarEstado");

    if (!modalNuevo || !form) {
        return;
    }

    const origenInput = document.getElementById("nuevoOrigen");
    const origenBotones = Array.from(
        modalNuevo.querySelectorAll("[data-origen]"));
    const campoAlerta = document.getElementById("campoAlertaOrigen");
    const alertaSelect = document.getElementById("nuevoCasoAlerta");
    const alertaCodigo = document.getElementById("alertaCodigoSeleccionado");
    const alertaDescripcion = document.getElementById(
        "alertaDescripcionSeleccionada");
    const btnSeleccionarAlerta = document.getElementById(
        "btnSeleccionarAlerta");
    const cedulaInput = document.getElementById("nuevoCasoCedula");
    const btnBuscar = document.getElementById("btnBuscarUsuarioCaso");
    const resultadoUsuario = document.getElementById("resultadoUsuarioCaso");
    const usuarioIniciales = document.getElementById("nuevoUsuarioIniciales");
    const usuarioNombre = document.getElementById("nuevoUsuarioNombre");
    const usuarioCedula = document.getElementById("nuevoUsuarioCedula");
    const usuarioHistorial = document.getElementById("nuevoUsuarioHistorial");
    const usuarioIdInput = document.getElementById(
        "nuevoCasoUsuarioEncontrado");
    const mensajeUsuario = document.getElementById("mensajeUsuarioCaso");
    const mensajeCrear = document.getElementById("mensajeCrearCaso");
    const estadoSelect = document.getElementById("nuevoCasoEstado");
    const descripcion = document.getElementById("nuevoCasoDescripcion");
    const btnGuardar = document.getElementById("btnGuardarNuevoCaso");

    const textoBotonGuardar = "Crear caso";
    const usuariosUrl = modalNuevo.dataset.usuariosUrl
        || "/personal/casos/usuarios/";
    const opcionesAlerta = Array.from(alertaSelect.options)
        .filter((opcion) => opcion.value !== "");
    const hayAlertas = opcionesAlerta.length > 0;

    let enviando = false;
    let usuarioActual = null;
    let alertaActual = null;

    // ---------------------------------------------------------------
    // Modales
    // ---------------------------------------------------------------

    function sincronizarScroll() {
        const abierto = document.querySelector(
            ".casos-modal-overlay.visible");
        document.body.style.overflow = abierto ? "hidden" : "";
    }

    function abrirModal(overlay, origen) {
        if (!overlay) {
            return;
        }

        if (overlay.id === "modal-nuevo" && origen) {
            aplicarOrigen(origen === "manual" ? "manual" : "alerta", true);
        }

        overlay.classList.add("visible");
        sincronizarScroll();

        const foco = overlay.id === "modal-nuevo"
            ? (origenInput.value === "alerta" && !btnSeleccionarAlerta.disabled
                ? btnSeleccionarAlerta
                : cedulaInput)
            : overlay.querySelector("button, input, select, textarea");

        if (foco) {
            window.setTimeout(() => foco.focus(), 0);
        }
    }

    function cerrarModal(overlay) {
        if (!overlay) {
            return;
        }

        overlay.classList.remove("visible");
        sincronizarScroll();
    }

    document.addEventListener("click", (evento) => {
        const abridor = evento.target.closest("[data-modal-open]");
        if (abridor) {
            evento.preventDefault();

            if (abridor.dataset.modalOpen === "modal-estado") {
                estadoCasoId.value = abridor.dataset.casoId;
                estadoCasoSelect.value = abridor.dataset.estadoId;
            }

            abrirModal(
                document.getElementById(abridor.dataset.modalOpen),
                abridor.dataset.origenInicial);
            return;
     }

        const cerrador = evento.target.closest("[data-modal-close]");
        if (cerrador) {
            evento.preventDefault();
            cerrarModal(cerrador.closest(".casos-modal-overlay"));
            return;
        }

        if (evento.target.classList.contains("casos-modal-overlay")) {
            cerrarModal(evento.target);
        }
    });

    document.addEventListener("keydown", (evento) => {
        if (evento.key !== "Escape") {
            return;
        }

        const visibles = Array.from(
            document.querySelectorAll(".casos-modal-overlay.visible"));

        if (visibles.length > 0) {
            cerrarModal(visibles[visibles.length - 1]);
        }
    });

    // ---------------------------------------------------------------
    // Utilidades
    // ---------------------------------------------------------------

    function mostrarMensaje(elemento, texto, tipo) {
        if (!elemento) {
            return;
        }

        elemento.classList.remove("error", "exito", "info");

        if (!texto) {
            elemento.textContent = "";
            elemento.style.display = "none";
            return;
        }

        elemento.textContent = texto;
        if (tipo) {
            elemento.classList.add(tipo);
        }
        elemento.style.display = "block";
    }

    function soloDigitos(valor) {
        return (valor || "").replace(/\D/g, "");
    }

    function iniciales(nombre) {
        const partes = (nombre || "").trim().split(/\s+/).filter(Boolean);

        if (partes.length === 0) {
            return "--";
        }
        if (partes.length === 1) {
            return partes[0].slice(0, 2).toUpperCase();
        }

        return (partes[0][0] + partes[1][0]).toUpperCase();
    }

    // ---------------------------------------------------------------
    // Origen del caso
    // ---------------------------------------------------------------

    function limpiarUsuario(limpiarCedula) {
        usuarioActual = null;
        usuarioIdInput.value = "";
        resultadoUsuario.style.display = "none";
        usuarioIniciales.textContent = "--";
        usuarioNombre.textContent = "";
        usuarioCedula.textContent = "";
        usuarioHistorial.textContent = "";

        if (limpiarCedula) {
            cedulaInput.value = "";
        }

        mostrarMensaje(mensajeUsuario, "", null);
    }

    function actualizarAlertaSeleccionada() {
        const opcion = alertaSelect.selectedOptions[0];

        if (!opcion || !opcion.value) {
            alertaActual = null;
            alertaCodigo.textContent = hayAlertas
                ? "Seleccione una alerta"
                : "Sin alertas disponibles";
            alertaDescripcion.textContent = hayAlertas
                ? "No hay una alerta seleccionada"
                : "No hay alertas sin caso asociado.";
            btnSeleccionarAlerta.textContent = hayAlertas
                ? "Seleccionar"
                : "Sin alertas";
            return;
        }

        alertaActual = {
            id: opcion.value,
            cedula: opcion.dataset.cedula || "",
            usuario: opcion.dataset.usuario || "",
            codigo: opcion.dataset.codigo || `ALT-${opcion.value}`,
            fecha: opcion.dataset.fecha || "",
            estado: opcion.dataset.estado || "",
            riesgo: opcion.dataset.riesgo || "",
            observaciones: opcion.dataset.observaciones || ""
        };

        alertaCodigo.textContent =
            `${alertaActual.codigo} · ${alertaActual.usuario}`;
        alertaDescripcion.textContent = [
            alertaActual.fecha,
            alertaActual.riesgo ? `Riesgo ${alertaActual.riesgo}` : "",
            alertaActual.estado
        ].filter(Boolean).join(" · ");
        btnSeleccionarAlerta.textContent = "Cambiar";
    }

    function aplicarOrigen(origen, limpiar) {
        const esAlerta = origen === "alerta";

        origenInput.value = esAlerta ? "alerta" : "manual";

        origenBotones.forEach((boton) => {
            boton.classList.toggle(
                "seleccionada", boton.dataset.origen === origenInput.value);
        });

        campoAlerta.style.display = esAlerta ? "" : "none";
        cedulaInput.readOnly = esAlerta;
        descripcion.required = !esAlerta;

        if (!limpiar) {
            return;
        }

        const observacionesPrevias =
            (alertaActual?.observaciones || "").trim();

        alertaSelect.value = "";
        alertaSelect.style.display = "none";
        actualizarAlertaSeleccionada();

        if (descripcion.value.trim() === observacionesPrevias) {
            descripcion.value = "";
        }

        limpiarUsuario(true);
        mostrarMensaje(mensajeCrear, "", null);

        if (esAlerta && !hayAlertas) {
            mostrarMensaje(
                mensajeCrear,
                "No hay alertas disponibles sin caso asociado. Use el registro manual.",
                "info");
        }
    }

    origenBotones.forEach((boton) => {
        boton.addEventListener("click", () => {
            if (origenInput.value === boton.dataset.origen) {
                return;
            }

            aplicarOrigen(boton.dataset.origen, true);
        });
    });

    // ---------------------------------------------------------------
    // Alerta de origen
    // ---------------------------------------------------------------

    if (!hayAlertas) {
        btnSeleccionarAlerta.disabled = true;
    }

    btnSeleccionarAlerta.addEventListener("click", () => {
        if (btnSeleccionarAlerta.disabled) {
            return;
        }

        const visible = alertaSelect.style.display !== "none";
        alertaSelect.style.display = visible ? "none" : "block";

        if (!visible) {
            alertaSelect.focus();
        }
    });

    alertaSelect.addEventListener("change", () => {
        const observacionesPrevias =
            (alertaActual?.observaciones || "").trim();
        const teniaAlerta = alertaActual !== null;

        actualizarAlertaSeleccionada();

        if (!alertaActual) {
            if (teniaAlerta
                && descripcion.value.trim() === observacionesPrevias) {
                descripcion.value = "";
            }

            cedulaInput.value = "";
            limpiarUsuario(false);
            return;
        }

        alertaSelect.style.display = "none";

        if (!descripcion.value.trim()
            || descripcion.value.trim() === observacionesPrevias) {
            descripcion.value = alertaActual.observaciones;
        }

        cedulaInput.value = soloDigitos(alertaActual.cedula);
        limpiarUsuario(false);
        buscarUsuario(true);
    });

    // ---------------------------------------------------------------
    // Búsqueda del usuario
    // ---------------------------------------------------------------

    async function buscarUsuario(auto) {
        const cedula = soloDigitos(cedulaInput.value);

        if (!cedula) {
            limpiarUsuario(false);
            mostrarMensaje(
                mensajeUsuario, "Ingrese una cédula válida.", "error");
            return;
        }

        cedulaInput.value = cedula;
        usuarioIdInput.value = "";
        usuarioActual = null;
        resultadoUsuario.style.display = "none";
        mostrarMensaje(mensajeUsuario, "", null);

        const textoOriginal = btnBuscar.textContent;
        btnBuscar.disabled = true;
        btnBuscar.textContent = "Buscando...";

        try {
            const respuesta = await fetch(
                `${usuariosUrl}${encodeURIComponent(cedula)}`,
                {
                    method: "GET",
                    headers: { "Accept": "application/json" },
                    credentials: "same-origin"
                });

            let datos = null;
            try {
                datos = await respuesta.json();
            } catch {
                datos = null;
            }

            if (!respuesta.ok || !datos) {
                mostrarMensaje(
                    mensajeUsuario,
                    datos?.mensaje
                        || "No se encontró un usuario con esa cédula.",
                    "error");
                return;
            }

            usuarioActual = datos;
            usuarioIdInput.value = String(datos.cedula);
            cedulaInput.value = String(datos.cedula);
            usuarioIniciales.textContent = iniciales(datos.nombre);
            usuarioNombre.textContent = datos.nombre || "";
            usuarioCedula.textContent = `Cédula ${datos.cedula}`;
            usuarioHistorial.textContent = "Usuario verificado";
            resultadoUsuario.style.display = "";

            if (!auto) {
                mostrarMensaje(
                    mensajeUsuario, "Usuario encontrado.", "exito");
            }
        } catch {
            mostrarMensaje(
                mensajeUsuario,
                "No se pudo verificar el usuario. Intente de nuevo.",
                "error");
        } finally {
            btnBuscar.disabled = false;
            btnBuscar.textContent = textoOriginal;
        }
    }

    btnBuscar.addEventListener("click", () => buscarUsuario(false));

    cedulaInput.addEventListener("input", () => {
        const limpio = soloDigitos(cedulaInput.value);

        if (cedulaInput.value !== limpio) {
            cedulaInput.value = limpio;
        }

        if (usuarioIdInput.value && usuarioIdInput.value !== limpio) {
            limpiarUsuario(false);
        }
    });

    cedulaInput.addEventListener("keydown", (evento) => {
        if (evento.key === "Enter") {
            evento.preventDefault();
            buscarUsuario(false);
        }
    });

    // ---------------------------------------------------------------
    // Envío del formulario
    // ---------------------------------------------------------------

    form.addEventListener("submit", (evento) => {
        if (enviando) {
            evento.preventDefault();
            return;
        }

        const esAlerta = origenInput.value === "alerta";
        const cedula = soloDigitos(cedulaInput.value);
        const idUsuario = soloDigitos(usuarioIdInput.value);
        const texto = descripcion.value.trim();
        const observacionesAlerta = esAlerta
            ? (alertaActual?.observaciones || "").trim()
            : "";
        const descripcionEfectiva = texto || observacionesAlerta;

        let error = "";

        if (esAlerta && !alertaActual) {
            error = "Seleccione la alerta de origen.";
        } else if (!cedula || !idUsuario || idUsuario !== cedula) {
            error = "Busque y seleccione un usuario válido.";
        } else if (!estadoSelect.value) {
            error = "Seleccione el estado inicial del caso.";
        } else if (descripcionEfectiva.length < 5) {
            error = "Escriba una descripción de al menos 5 caracteres.";
        }

        if (error) {
            evento.preventDefault();
            mostrarMensaje(mensajeCrear, error, "error");
            return;
        }

        mostrarMensaje(mensajeCrear, "", null);
        enviando = true;
        btnGuardar.disabled = true;
        btnGuardar.textContent = "Creando caso...";
    });

    window.addEventListener("pageshow", () => {
        enviando = false;
        btnGuardar.disabled = false;
        btnGuardar.textContent = textoBotonGuardar;
    });

    // ---------------------------------------------------------------
    // Cambio de estado
    // ---------------------------------------------------------------

    if (formEstado) {
        formEstado.addEventListener("submit", () => {
            btnGuardarEstado.disabled = true;
            btnGuardarEstado.textContent = "Guardando...";
        });
    }

    // ---------------------------------------------------------------
    // Inicio
    // ---------------------------------------------------------------

    aplicarOrigen(origenInput.value === "alerta" ? "alerta" : "manual", false);
    actualizarAlertaSeleccionada();

    if (modalNuevo.dataset.abrirModal === "true") {
        abrirModal(modalNuevo);
    }
})();
