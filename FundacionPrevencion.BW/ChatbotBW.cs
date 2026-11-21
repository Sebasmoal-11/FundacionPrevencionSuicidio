
using FundacionPrevencion.DA;
using Proyecto.Models;
using FundacionPrevencion.BW.Servicios;

namespace FundacionPrevencion.BW
{
    public class ChatbotBW
    {
        private readonly ChatbotDA _chatbotDA;
        private readonly GeminiService _geminiService;
        public ChatbotBW(
     ChatbotDA chatbotDA,
     GeminiService geminiService)
        {
            _chatbotDA = chatbotDA;
            _geminiService = geminiService;
        }

        // Consultar las conversaciones de un usuario
        public async Task<List<Conversacionchatbot>> ObtenerConversacionesUsuario(
            int cedulaUsuario)
        {
            if (cedulaUsuario <= 0)
            {
                throw new ArgumentException("La cédula del usuario no es válida.");
            }

            return await _chatbotDA.ObtenerConversacionesUsuario(cedulaUsuario);
        }

        // Consultar una conversación
        public async Task<Conversacionchatbot?> ObtenerConversacion(
            int idConversacion, int cedulaUsuario)
        {
            if (idConversacion <= 0 || cedulaUsuario <= 0)
            {
                throw new ArgumentException("Los datos de la consulta no son válidos.");
            }

            return await _chatbotDA.ObtenerConversacion(
                idConversacion, cedulaUsuario);
        }

        // Consultar los mensajes de una conversación
        public async Task<List<Mensajechatbot>> ObtenerMensajesConversacion(
            int idConversacion, int cedulaUsuario)
        {
            if (idConversacion <= 0 || cedulaUsuario <= 0)
            {
                throw new ArgumentException("Los datos de la consulta no son válidos.");
            }

            return await _chatbotDA.ObtenerMensajesConversacion(
                idConversacion, cedulaUsuario);
        }

        // Actualizar la actividad de una conversación
        public async Task<bool> ActualizarFechaActividad(
            int idConversacion, int cedulaUsuario)
        {
            if (idConversacion <= 0 || cedulaUsuario <= 0)
            {
                throw new ArgumentException("Los datos de la consulta no son válidos.");
            }

            return await _chatbotDA.ActualizarFechaActividad(
                idConversacion, cedulaUsuario);
        }
        public async Task<string> EnviarMensajeGemini(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío.");

            return await _geminiService.EnviarMensaje(mensaje);
        }
        // Crear una conversación
        public async Task<Conversacionchatbot> CrearConversacion(
            int cedulaUsuario)
        {
            return await _chatbotDA.CrearConversacion(cedulaUsuario);
        }

        // Guardar un mensaje
        public async Task<Mensajechatbot> GuardarMensaje(
            int idConversacion,
            int idTipoEmisor,
            string contenido)
        {
            return await _chatbotDA.GuardarMensaje(
                idConversacion,
                idTipoEmisor,
                contenido);
        }

        // Obtener conversación con sus mensajes
        public async Task<Conversacionchatbot?> ObtenerConversacionConMensajes(
            int idConversacion,
            int cedulaUsuario)
        {
            return await _chatbotDA.ObtenerConversacionConMensajes(
                idConversacion,
                cedulaUsuario);
        }
        public async Task<string> ProcesarMensaje(
    int idConversacion,
    int cedulaUsuario,
    string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje no puede estar vacío.");

            // Verificar que la conversación pertenezca al usuario
            var conversacion = await _chatbotDA.ObtenerConversacion(
                idConversacion, cedulaUsuario);

            if (conversacion == null)
                throw new InvalidOperationException(
                    "La conversación no existe o no pertenece al usuario.");

            // Guardar el mensaje enviado por el usuario
            await _chatbotDA.GuardarMensaje(idConversacion, 1, mensaje);

            // Recuperar todos los mensajes de la conversación
            var historial = await _chatbotDA.ObtenerMensajesConversacion(
                idConversacion, cedulaUsuario);

            // Enviar el historial completo a Gemini
            string respuesta = await _geminiService.EnviarMensaje(historial);

            // Guardar la respuesta generada por el chatbot
            await _chatbotDA.GuardarMensaje(idConversacion, 2, respuesta);

            // Actualizar la última actividad
            await _chatbotDA.ActualizarFechaActividad(
                idConversacion, cedulaUsuario);

            return respuesta;
        }

        // Eliminar una conversación
        public async Task<bool> EliminarConversacion(
            int idConversacion,
            int cedulaUsuario)
        {
            if (idConversacion <= 0 || cedulaUsuario <= 0)
            {
                throw new ArgumentException(
                    "Los datos de la conversación no son válidos.");
            }

            return await _chatbotDA.EliminarConversacion(
                idConversacion,
                cedulaUsuario);
        }

    }
}