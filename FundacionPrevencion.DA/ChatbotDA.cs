
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace FundacionPrevencion.DA
{
    public class ChatbotDA
    {
        private readonly ApplicationDbContext _context;

        public ChatbotDA(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtener conversaciones del usuario con el primer mensaje de cada una
        public async Task<List<Conversacionchatbot>> ObtenerConversacionesUsuario(
            int cedulaUsuario)
        {
            return await _context.Conversacionchatbots
                .AsNoTracking()
                .Where(c => c.CedulaUsuario == cedulaUsuario)
                .Include(c => c.Mensajechatbots
                    .Where(m => m.IdTipoEmisor == 1)
                    .OrderBy(m => m.FechaEnvio)
                    .ThenBy(m => m.IdMensaje)
                    .Take(1))
                .OrderByDescending(c => c.FechaUltimaActividad)
                .ToListAsync();
        }

        // Obtener una conversación verificando que pertenezca al usuario
        public async Task<Conversacionchatbot?> ObtenerConversacion(
            int idConversacion, int cedulaUsuario)
        {
            return await _context.Conversacionchatbots
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.IdConversacion == idConversacion &&
                    c.CedulaUsuario == cedulaUsuario);
        }

        // Obtener los mensajes de una conversación del usuario
        public async Task<List<Mensajechatbot>> ObtenerMensajesConversacion(
            int idConversacion, int cedulaUsuario)
        {
            return await _context.Mensajechatbots
                .AsNoTracking()
                .Where(m =>
                    m.IdConversacion == idConversacion &&
                    m.IdConversacionNavigation.CedulaUsuario == cedulaUsuario)
                .OrderBy(m => m.FechaEnvio)
                .ThenBy(m => m.IdMensaje)
                .ToListAsync();
        }

        // Actualizar la fecha de última actividad
        public async Task<bool> ActualizarFechaActividad(
            int idConversacion, int cedulaUsuario)
        {
            var conversacion = await _context.Conversacionchatbots
                .FirstOrDefaultAsync(c =>
                    c.IdConversacion == idConversacion &&
                    c.CedulaUsuario == cedulaUsuario);

            if (conversacion == null)
            {
                return false;
            }

            conversacion.FechaUltimaActividad = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        // Crear una conversación nueva
        public async Task<Conversacionchatbot> CrearConversacion(
            int cedulaUsuario)
        {
            var conversacion = new Conversacionchatbot
            {
                CedulaUsuario = cedulaUsuario,
                FechaInicio = DateTime.Now,
                FechaUltimaActividad = DateTime.Now,
                IdEstadoActivo = 1
            };

            _context.Conversacionchatbots.Add(conversacion);

            await _context.SaveChangesAsync();

            return conversacion;
        }

        // Guardar un mensaje
        public async Task<Mensajechatbot> GuardarMensaje(
            int idConversacion,
            int idTipoEmisor,
            string contenido)
        {
            var mensaje = new Mensajechatbot
            {
                IdConversacion = idConversacion,
                IdTipoEmisor = idTipoEmisor,
                Contenido = contenido,
                FechaEnvio = DateTime.Now
            };

            _context.Mensajechatbots.Add(mensaje);

            await _context.SaveChangesAsync();

            return mensaje;
        }

        // Obtener una conversación con sus mensajes
        public async Task<Conversacionchatbot?> ObtenerConversacionConMensajes(
            int idConversacion,
            int cedulaUsuario)
        {
            return await _context.Conversacionchatbots
                .Include(c => c.Mensajechatbots)
                .FirstOrDefaultAsync(c =>
                    c.IdConversacion == idConversacion &&
                    c.CedulaUsuario == cedulaUsuario);
        }

        // Eliminar una conversación y todos sus mensajes
        public async Task<bool> EliminarConversacion(
            int idConversacion,
            int cedulaUsuario)
        {
            var conversacion = await _context.Conversacionchatbots
                .Include(c => c.Mensajechatbots)
                .FirstOrDefaultAsync(c =>
                    c.IdConversacion == idConversacion &&
                    c.CedulaUsuario == cedulaUsuario);

            if (conversacion == null)
            {
                return false;
            }

            // Eliminar primero los mensajes asociados
            _context.Mensajechatbots.RemoveRange(
                conversacion.Mensajechatbots);

            // Eliminar la conversación
            _context.Conversacionchatbots.Remove(conversacion);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}