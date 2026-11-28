// Gestión de casos: detalle real, reasignación y cambio de estado.
(() => {
    "use strict";

    const modalDetalle = document.getElementById("modal-detalle");
    const modalReasignar = document.getElementById("modal-reasignar");
    const modalEstado = document.getElementById("modal-estado");

    if (!modalDetalle || !modalReasignar || !modalEstado) {
        return;
    }

    const detalleCodigo = document.getElementById("detalleCodigo");
    const detalleEstado = document.getElementById("detalleEstado");
    const detalleUsuario = document.getElementById("detalleUsuario");
    const detalleCedula = document.getElementById("detalleCedula");
    const detalleFecha = document.getElementById("detalleFecha");
    const detalleResponsable = document.getElementById("detalleResponsable");
    const detalleDescripcion = document.getElementById("detalleDescripcion");

    const detalleSeccionAlerta = document.getElementById(
        "detalleSeccionAlerta");

    const detalleCodigoAlerta = document.getElementById(
        "detalleCodigoAlerta");

    const detalleInfoAlerta = document.getElementById(
        "detalleInfoAlerta");

    const detalleObservacionesAlerta = document.getElementById(
        "detalleObservacionesAlerta");

    const detalleHistorial = document.getElementById(
        "detalleHistorial");

    const mensajeDetalle = document.getElementById(
        "mensajeDetalleCaso");

    const reasignarCasoId = document.getElementById(
        "reasignarCasoId");

    const reasignarEmpleado = document.getElementById(
        "reasignarEmpleado");

    const formReasignar = document.getElementById(
        "formReasignarCaso");

    const btnConfirmarReasignacion = document.getElementById(
        "btnConfirmarReasignacion");

    const mensajeReasignacion = document.getElementById(
        "mensajeReasignarCaso");

    const estadoCasoId = document.getElementById(
        "estadoCasoId");

    const estadoCasoSelect = document.getElementById(
        "estadoCasoSelect");

    const btnCambiarEstadoDetalle = document.getElementById(
        "btnCambiarEstadoDetalle");

    const btnAbrirReasignacion = document.getElementById(
        "btnAbrirReasignacion");

    let casoActual = null;
    let cargandoDetalle = false;

    // ------------------------------------------------------------
    // Utilidades
    // ------------------------------------------------------------

    function mostrarMensaje(elemento, texto, tipo) {
        if (!elemento) {
            return;
        }

        elemento.textContent = texto || "";
        elemento.classList.remove("error", "exito", "info");

        if (!texto) {
            elemento.style.display = "none";
            return;
        }

        if (tipo) {
            elemento.classList.add(tipo);
        }

        elemento.style.display = "block";
    }

    function abrirModal(modal) {
        if (!modal) {
            return;
        }

        modal.classList.add("visible");
        document.body.style.overflow = "hidden";
    }

    function cerrarModal(modal) {
        if (!modal) {
            return;
        }

        modal.classList.remove("visible");

        if (!document.querySelector(
            ".casos-modal-overlay.visible")) {
            document.body.style.overflow = "";
        }
    }

    function nombreClaseEstado(nombre) {
        switch (nombre) {
            case "Abierto":
                return "estado-abierto";

            case "En seguimiento":
                return "estado-seguimiento";

            case "Cerrado":
                return "estado-cerrado";

            default:
                return "estado-cerrado";
        }
    }

    function mostrarEstado(nombre) {
        detalleEstado.textContent = nombre || "Sin estado";

        detalleEstado.className =
            "estado-badge " + nombreClaseEstado(nombre);
    }

    // ------------------------------------------------------------
    // Historial
    // ------------------------------------------------------------

    function cargarHistorial(asignaciones) {
        detalleHistorial.replaceChildren();

        if (!asignaciones || asignaciones.length === 0) {
            const mensaje = document.createElement("p");

            mensaje.textContent = "No hay asignaciones registradas.";

            detalleHistorial.appendChild(mensaje);
            return;
        }

        asignaciones.forEach((asignacion) => {
            const elemento = document.createElement("div");
            elemento.className = "timeline-item";

            const titulo = document.createElement("strong");
            titulo.textContent = asignacion.empleado;

            const fecha = document.createElement("small");
            fecha.textContent = asignacion.fecha;

            const estado = document.createElement("small");
            estado.textContent = "Estado: " + asignacion.estado;

            elemento.appendChild(titulo);
            elemento.appendChild(fecha);
            elemento.appendChild(estado);

            detalleHistorial.appendChild(elemento);
        });
    }

    // ------------------------------------------------------------
    // Mostrar detalles reales
    // ------------------------------------------------------------

    async function cargarDetalle(idCaso) {
        if (cargandoDetalle) {
            return;
        }

        cargandoDetalle = true;
        casoActual = idCaso;

        detalleCodigo.textContent = `CAS-${idCaso}`;
        mostrarEstado("Cargando...");

        detalleUsuario.textContent = "Cargando...";
        detalleCedula.textContent = "Cargando...";
        detalleFecha.textContent = "Cargando...";
        detalleResponsable.textContent = "Cargando...";
        detalleDescripcion.textContent = "Cargando...";
        detalleHistorial.replaceChildren();

        detalleSeccionAlerta.style.display = "none";

        mostrarMensaje(mensajeDetalle, "", null);

        abrirModal(modalDetalle);

        try {
            const respuesta = await fetch(
                `/Personal/ObtenerDetalleCaso?idCaso=${encodeURIComponent(idCaso)}`,
                {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    credentials: "same-origin"
                });

            const datos = await respuesta.json();

            if (!respuesta.ok) {
                throw new Error(
                    datos?.mensaje || "No se pudo consultar el caso.");
            }

            detalleCodigo.textContent = datos.codigo;
            mostrarEstado(datos.estado);

            detalleUsuario.textContent = datos.usuario;
            detalleCedula.textContent = datos.cedula;
            detalleFecha.textContent = datos.fechaApertura;
            detalleResponsable.textContent = datos.responsable;
            detalleDescripcion.textContent = datos.descripcion;

            if (datos.desdeAlerta) {
                detalleSeccionAlerta.style.display = "";

                detalleCodigoAlerta.textContent =
                    datos.codigoAlerta || "Alerta asociada";

                detalleInfoAlerta.textContent = [
                    datos.estadoAlerta,
                    datos.nivelRiesgo
                        ? `Nivel de riesgo: ${datos.nivelRiesgo}`
                        : ""
                ].filter(Boolean).join(" · ");

                detalleObservacionesAlerta.textContent =
                    datos.observacionesAlerta || "Sin observaciones.";
            } else {
                detalleSeccionAlerta.style.display = "none";
            }

            cargarHistorial(datos.historialAsignaciones);

        } catch (error) {
            mostrarEstado("Error");

            mostrarMensaje(
                mensajeDetalle,
                error.message || "No se pudo cargar la información del caso.",
                "error");

            detalleDescripcion.textContent =
                "No se pudo cargar la información.";
        } finally {
            cargandoDetalle = false;
        }
    }

    // ------------------------------------------------------------
    // Abrir detalles
    // ------------------------------------------------------------

    document.addEventListener("click", (evento) => {
        const botonVer = evento.target.closest(
            '[data-modal-open="modal-detalle"]');

        if (!botonVer) {
            return;
        }

        const idCaso = botonVer.dataset.casoId;

        if (!idCaso) {
            return;
        }

        evento.preventDefault();

        cargarDetalle(idCaso);
    });

    // ------------------------------------------------------------
    // Reasignar
    // ------------------------------------------------------------

    btnAbrirReasignacion.addEventListener("click", () => {
        if (!casoActual) {
            return;
        }

        reasignarCasoId.value = casoActual;

        reasignarEmpleado.value = "";
        mostrarMensaje(mensajeReasignacion, "", null);

        cerrarModal(modalDetalle);
        abrirModal(modalReasignar);
    });

   formReasignar.addEventListener("submit", (evento) => {
    if (!reasignarCasoId.value || !reasignarEmpleado.value) {
        evento.preventDefault();

        mostrarMensaje(
            mensajeReasignacion,
            "Complete todos los campos.",
            "error");

        return;
    }

    btnConfirmarReasignacion.disabled = true;
    btnConfirmarReasignacion.textContent = "Reasignando...";
    });

    // ------------------------------------------------------------
    // Cambiar estado desde detalle
    // ------------------------------------------------------------

    btnCambiarEstadoDetalle.addEventListener("click", () => {
        if (!casoActual) {
            return;
        }

        estadoCasoId.value = casoActual;

        const estadoActual = detalleEstado.textContent.trim();

        const opciones = Array.from(estadoCasoSelect.options);

        const opcionActual = opciones.find(
            opcion => opcion.text.trim() === estadoActual);

        if (opcionActual) {
            estadoCasoSelect.value = opcionActual.value;
        }

        cerrarModal(modalDetalle);
        abrirModal(modalEstado);
    });

    // ------------------------------------------------------------
    // Enviar cambio de estado
    // ------------------------------------------------------------

    const formEstado = document.getElementById("formCambiarEstado");

    const btnGuardarEstado = document.getElementById(
        "btnGuardarEstado");

    if (formEstado) {
        formEstado.addEventListener("submit", () => {
            btnGuardarEstado.disabled = true;
            btnGuardarEstado.textContent = "Guardando...";
        });
    }

})();