
using FundacionPrevencion.BW;
using Proyecto.Models;

namespace FundacionPrevencion.BC
{
    public class ChatbotBC
    {
        private readonly ChatbotBW _chatbotBW;

        public ChatbotBC(ChatbotBW chatbotBW)
        {
            _chatbotBW = chatbotBW;
        }

        // Obtener las conversaciones de un usuario
        public async Task<List<Conversacionchatbot>> ObtenerConversacionesUsuario(
            int cedulaUsuario)
        {
            return await _chatbotBW.ObtenerConversacionesUsuario(cedulaUsuario);
        }

        // Obtener una conversación específica
        public async Task<Conversacionchatbot?> ObtenerConversacion(
            int idConversacion, int cedulaUsuario)
        {
            return await _chatbotBW.ObtenerConversacion(
                idConversacion, cedulaUsuario);
        }

        // Obtener los mensajes de una conversación
        public async Task<List<Mensajechatbot>> ObtenerMensajesConversacion(
            int idConversacion, int cedulaUsuario)
        {
            return await _chatbotBW.ObtenerMensajesConversacion(
                idConversacion, cedulaUsuario);
        }

        // Actualizar la última actividad
        public async Task<bool> ActualizarFechaActividad(
            int idConversacion, int cedulaUsuario)
        {
            return await _chatbotBW.ActualizarFechaActividad(
                idConversacion, cedulaUsuario);
        }
        public async Task<string> EnviarMensajeGemini(string mensaje)
        {
            return await _chatbotBW.EnviarMensajeGemini(mensaje);
        }
        // Crear una conversación
        public async Task<Conversacionchatbot> CrearConversacion(
            int cedulaUsuario)
        {
            return await _chatbotBW.CrearConversacion(cedulaUsuario);
        }

        // Guardar un mensaje
        public async Task<Mensajechatbot> GuardarMensaje(
            int idConversacion,
            int idTipoEmisor,
            string contenido)
        {
            return await _chatbotBW.GuardarMensaje(
                idConversacion,
                idTipoEmisor,
                contenido);
        }

        // Obtener conversación con sus mensajes
        public async Task<Conversacionchatbot?> ObtenerConversacionConMensajes(
            int idConversacion,
            int cedulaUsuario)
        {
            return await _chatbotBW.ObtenerConversacionConMensajes(
                idConversacion,
                cedulaUsuario);
        }
        public async Task<string> ProcesarMensaje(
    int idConversacion,
    int cedulaUsuario,
    string mensaje)
        {
            return await _chatbotBW.ProcesarMensaje(
                idConversacion,
                cedulaUsuario,
                mensaje);
        }
        // Eliminar una conversación
        public async Task<bool> EliminarConversacion(
            int idConversacion,
            int cedulaUsuario)
        {
            return await _chatbotBW.EliminarConversacion(
                idConversacion,
                cedulaUsuario);
        }
    }
}
