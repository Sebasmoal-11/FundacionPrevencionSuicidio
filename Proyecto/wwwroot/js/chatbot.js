
//  TEMPORAL: reemplazar por la cédula del usuario autenticado.
const tokenCsrf = document.querySelector("#chatbot-antiforgery input").value;

async function solicitarChat(url, opciones = {}) {
    const respuesta = await fetch(url, {
        ...opciones,
        headers: { ...opciones.headers, "X-CSRF-TOKEN": tokenCsrf }
    });
    if (respuesta.status === 401) {
        window.location.assign("/ingreso");
        throw new Error("Tu sesión venció.");
    }
    return respuesta;
}

let idConversacionActual = null;

// Elementos de la interfaz
const btnNuevaConversacion = document.getElementById("btnNuevaConversacion");
const btnEnviarMensaje = document.getElementById("btnEnviarMensaje");
const mensajeInput = document.getElementById("mensajeInput");
const areaMensajes = document.getElementById("areaMensajes");
const listaConversaciones = document.getElementById("listaConversaciones");


// INICIALIZACIÓN


document.addEventListener("DOMContentLoaded", async () => {
    await cargarConversaciones();
});


// FORMATEAR FECHA Y HORA


function formatearFecha(fecha) {
    const fechaConversacion = new Date(fecha);

    if (isNaN(fechaConversacion.getTime())) {
        return "Fecha no disponible";
    }

    return fechaConversacion.toLocaleString("es-CR", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        hour12: true
    });
}


// CARGAR CONVERSACIONES


async function cargarConversaciones() {
    try {
        const respuesta = await solicitarChat(
            "/Chatbot/ObtenerConversaciones"
        );

        if (!respuesta.ok) {
            throw new Error("No se pudieron cargar las conversaciones.");
        }

        const conversaciones = await respuesta.json();

        listaConversaciones.innerHTML = "";

        if (conversaciones.length === 0) {
            idConversacionActual = null;
            mostrarBienvenida();
            return;
        }

        conversaciones.forEach(conversacion => {
            agregarConversacionLista(conversacion);
        });

        // Seleccionar la conversación más reciente.
        await seleccionarConversacion(conversaciones[0].idConversacion);

    } catch (error) {
        console.error(error);
        mostrarError("No fue posible cargar las conversaciones.");
    }
}


// MOSTRAR CONVERSACIONES EN EL PANEL


function agregarConversacionLista(conversacion) {
    const elemento = document.createElement("div");

    elemento.className = "conversation-item";
    elemento.dataset.id = conversacion.idConversacion;
    elemento.setAttribute("role", "button");
    elemento.setAttribute("tabindex", "0");

    const contenido = document.createElement("div");
    contenido.className = "conversation-content";

    const titulo = document.createElement("div");
    titulo.className = "conversation-title";
    titulo.textContent = conversacion.titulo || "Nueva conversación";
    titulo.title = conversacion.titulo || "Nueva conversación";

    const fecha = document.createElement("small");
    fecha.className = "conversation-date";
    fecha.textContent = formatearFecha(conversacion.fechaInicio);

    contenido.appendChild(titulo);
    contenido.appendChild(fecha);

    const btnEliminar = document.createElement("button");
    btnEliminar.type = "button";
    btnEliminar.className = "delete-conversation";
    btnEliminar.innerHTML = '<i class="bi bi-trash"></i>';
    btnEliminar.title = "Eliminar conversación";
    btnEliminar.setAttribute("aria-label", "Eliminar conversación");

    btnEliminar.addEventListener("click", event => {
        event.stopPropagation();
        eliminarConversacion(conversacion.idConversacion);
    });

    elemento.appendChild(contenido);
    elemento.appendChild(btnEliminar);

    elemento.addEventListener("click", () => {
        seleccionarConversacion(conversacion.idConversacion);
    });

    elemento.addEventListener("keydown", event => {
        if (event.key === "Enter" || event.key === " ") {
            event.preventDefault();
            seleccionarConversacion(conversacion.idConversacion);
        }
    });

    listaConversaciones.appendChild(elemento);
}


// SELECCIONAR CONVERSACIÓN
 

async function seleccionarConversacion(idConversacion) {
    try {
        const respuesta = await solicitarChat(
            `/Chatbot/ObtenerMensajes/${idConversacion}`
        );

        if (!respuesta.ok) {
            throw new Error("No se pudo recuperar la conversación.");
        }

        const datos = await respuesta.json();

        idConversacionActual = datos.idConversacion;

        actualizarConversacionActiva();

        areaMensajes.innerHTML = "";

        if (datos.mensajes.length === 0) {
            mostrarBienvenida();
            return;
        }

        datos.mensajes.forEach(mensaje => {
            mostrarMensaje(
                mensaje.contenido,
                mensaje.idTipoEmisor === 1 ? "user" : "bot"
            );
        });

    } catch (error) {
        console.error(error);
        mostrarError("No fue posible abrir esta conversación.");
    }
}


// RESALTAR CONVERSACIÓN SELECCIONADA


function actualizarConversacionActiva() {
    document.querySelectorAll(".conversation-item").forEach(elemento => {
        elemento.classList.toggle(
            "active",
            Number(elemento.dataset.id) === idConversacionActual
        );
    });
}


// CREAR NUEVA CONVERSACIÓN


async function crearConversacion() {
    try {
        btnNuevaConversacion.disabled = true;

        const respuesta = await solicitarChat("/Chatbot/IniciarConversacion", {
            method: "POST"
        });

        const datos = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error(datos.mensaje || "No se pudo crear la conversación.");
        }

        idConversacionActual = datos.idConversacion;

        areaMensajes.innerHTML = "";
        mostrarBienvenida();

        await cargarConversaciones();

    } catch (error) {
        console.error(error);
        mostrarError(error.message);
    } finally {
        btnNuevaConversacion.disabled = false;
    }
}


// ENVIAR MENSAJE


async function enviarMensaje() {
    const mensaje = mensajeInput.value.trim();

    if (!mensaje) {
        return;
    }

    let indicador = null;

    try {
        btnEnviarMensaje.disabled = true;
        mensajeInput.disabled = true;

        // Crear conversación si todavía no existe.
        if (!idConversacionActual) {
            const respuestaNueva = await solicitarChat("/Chatbot/IniciarConversacion", {
                method: "POST"
            });

            const nuevaConversacion = await respuestaNueva.json();

            if (!respuestaNueva.ok) {
                throw new Error(
                    nuevaConversacion.mensaje ||
                    "No se pudo iniciar la conversación."
                );
            }

            idConversacionActual = nuevaConversacion.idConversacion;
        }

        mensajeInput.value = "";
        mostrarMensaje(mensaje, "user");

        indicador = mostrarIndicadorEscritura();

        const respuesta = await solicitarChat("/Chatbot/EnviarMensaje", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                idConversacion: idConversacionActual,
                mensaje: mensaje
            })
        });

        const datos = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error(datos.mensaje || "No se pudo enviar el mensaje.");
        }

        mostrarMensaje(datos.respuesta, "bot");

        await cargarConversaciones();

    } catch (error) {
        console.error(error);
        mostrarError(error.message);
    } finally {
        if (indicador) {
            indicador.remove();
        }

        btnEnviarMensaje.disabled = false;
        mensajeInput.disabled = false;
        mensajeInput.focus();
    }
}


// MOSTRAR MENSAJES


function mostrarMensaje(contenido, tipo) {
    const fila = document.createElement("div");
    fila.className = `message-row ${tipo}-message`;

    const burbuja = document.createElement("div");
    burbuja.className = "message-bubble";
    burbuja.textContent = contenido;

    fila.appendChild(burbuja);
    areaMensajes.appendChild(fila);

    areaMensajes.scrollTop = areaMensajes.scrollHeight;
}


// INDICADOR DE ESCRITURA


function mostrarIndicadorEscritura() {
    const indicador = document.createElement("div");

    indicador.className = "message-row bot-message";
    indicador.textContent = "El asistente está escribiendo...";

    areaMensajes.appendChild(indicador);
    areaMensajes.scrollTop = areaMensajes.scrollHeight;

    return indicador;
}

// BIENVENIDA


function mostrarBienvenida() {
    areaMensajes.innerHTML = "";

    const bienvenida = document.createElement("div");
    bienvenida.className = "welcome-message";

    const titulo = document.createElement("h3");
    titulo.textContent = "¡Hola! Bienvenido";

    const parrafo1 = document.createElement("p");
    parrafo1.textContent =
        "Soy tu asistente virtual. Estoy aquí para escucharte, orientarte y ofrecerte un espacio de confianza para conversar.";

    const parrafo2 = document.createElement("p");
    parrafo2.textContent = "¿Qué te gustaría conversar hoy?";

    bienvenida.append(titulo, parrafo1, parrafo2);
    areaMensajes.appendChild(bienvenida);
}


// MOSTRAR ERRORES


function mostrarError(mensaje) {
    const error = document.createElement("div");

    error.className = "chat-error";
    error.textContent = mensaje;

    areaMensajes.appendChild(error);
    areaMensajes.scrollTop = areaMensajes.scrollHeight;
}

// EVENTOS


btnNuevaConversacion.addEventListener("click", crearConversacion);

btnEnviarMensaje.addEventListener("click", enviarMensaje);

mensajeInput.addEventListener("keydown", event => {
    if (event.key === "Enter" && !event.shiftKey) {
        event.preventDefault();
        enviarMensaje();
    }
});



// ELIMINAR CONVERSACIÓN


async function eliminarConversacion(idConversacion) {
    const confirmar = confirm(
        "¿Estás seguro de que deseas eliminar esta conversación? También se eliminarán todos sus mensajes."
    );

    if (!confirmar) {
        return;
    }

    try {
        const respuesta = await solicitarChat(
            `/Chatbot/EliminarConversacion/${idConversacion}`,
            {
                method: "DELETE"
            }
        );

        const datos = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error(
                datos.mensaje || "No se pudo eliminar la conversación."
            );
        }

        // Si se eliminó la conversación abierta, limpiar el chat.
        if (idConversacionActual === idConversacion) {
            idConversacionActual = null;
            mostrarBienvenida();
        }

        // Actualizar la lista de conversaciones.
        await cargarConversaciones();

    } catch (error) {
        console.error(error);
        mostrarError(error.message);
    }
}

