
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


        // Obtener conversaciones visibles del usuario con el primer mensaje de cada una
        public async Task<List<Conversacionchatbot>> ObtenerConversacionesUsuario(
            int cedulaUsuario)
        {
            return await _context.Conversacionchatbots
                .AsNoTracking()
                .Where(c =>
                    c.CedulaUsuario == cedulaUsuario &&
                    !c.OcultaUsuario)
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
                IdEstadoActivo = await _context.Estadoactivos
                    .Where(estado => estado.Nombre == "Activo")
                    .Select(estado => estado.IdEstadoActivo)
                    .SingleAsync()
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


        // Ocultar una conversación del historial del usuario
        public async Task<bool> EliminarConversacion(
            int idConversacion,
            int cedulaUsuario)
        {
            var conversacion = await _context.Conversacionchatbots
                .FirstOrDefaultAsync(c =>
                    c.IdConversacion == idConversacion &&
                    c.CedulaUsuario == cedulaUsuario &&
                    !c.OcultaUsuario);

            if (conversacion == null)
            {
                return false;
            }

            conversacion.OcultaUsuario = true;

            await _context.SaveChangesAsync();

            return true;
        }

        // Guardar el resultado del análisis de riesgo
        public async Task<Resultadoanalisi> GuardarResultadoAnalisis(
            int idMensaje,
            int idNivelRiesgo,
            string resultado)
        {
            var analisis = new Resultadoanalisi
            {
                IdMensaje = idMensaje,
                IdNivelRiesgo = idNivelRiesgo,
                Resultado = resultado,
                FechaAnalisis = DateTime.Now
            };

            _context.Resultadoanalises.Add(analisis);

            await _context.SaveChangesAsync();

            return analisis;
        }
        // Crear una alerta asociada a un análisis de riesgo
        public async Task<Alertum> CrearAlerta(
            int idAnalisis,
            string observaciones)
        {
            // Evitar generar alertas duplicadas para el mismo análisis
            var alertaExistente = await _context.Alerta
                .FirstOrDefaultAsync(a => a.IdAnalisis == idAnalisis);

            if (alertaExistente != null)
            {
                return alertaExistente;
            }

            var alerta = new Alertum
            {
                IdAnalisis = idAnalisis,
                IdEstadoAlerta = 1, // Pendiente
                FechaGeneracion = DateTime.Now,
                Observaciones = observaciones
            };

            _context.Alerta.Add(alerta);

            await _context.SaveChangesAsync();

            return alerta;
        }
    }
}