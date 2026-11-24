
using FundacionPrevencion.BC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Proyecto.Autenticacion;


namespace Proyecto.Controllers
{
    [ApiController]
    [Authorize(Policy = "CuentaUsuario")]
    [AutoValidateAntiforgeryToken]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Route("Chatbot")]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatbotBC _chatbotBC;

        public ChatbotController(ChatbotBC chatbotBC)
        {
            _chatbotBC = chatbotBC;
        }

        private int CedulaUsuario => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Iniciar una conversación
        [HttpPost("IniciarConversacion")]
        public async Task<IActionResult> IniciarConversacion()
        {
            try
            {
                var conversacion = await _chatbotBC.CrearConversacion(
                    CedulaUsuario);

                return Ok(new
                {
                    mensaje = "Conversación iniciada correctamente.",
                    idConversacion = conversacion.IdConversacion,
                    fechaInicio = conversacion.FechaInicio
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo completar la operación."
                });
            }
        }

        // Enviar mensaje al chatbot
        [HttpPost("EnviarMensaje")]
        public async Task<IActionResult> EnviarMensaje(
            [FromBody] EnviarMensajeChatbotRequest solicitud)
        {
            if (solicitud == null ||
                solicitud.IdConversacion <= 0 ||
                string.IsNullOrWhiteSpace(solicitud.Mensaje))
            {
                return BadRequest(new
                {
                    mensaje = "Debes proporcionar una conversación y un mensaje válidos."
                });
            }

            try
            {
                string respuesta = await _chatbotBC.ProcesarMensaje(
                    solicitud.IdConversacion,
                    CedulaUsuario,
                    solicitud.Mensaje);

                return Ok(new
                {
                    respuesta = respuesta
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new { mensaje = "El servicio de conversación no está disponible." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "No se pudo completar la operación." });
            }
        }

        // Obtener mensajes de una conversación
        [HttpGet("ObtenerMensajes/{idConversacion}")]
        public async Task<IActionResult> ObtenerMensajes(
            int idConversacion)
        {
            if (idConversacion <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Los datos proporcionados no son válidos."
                });
            }

            try
            {
                var conversacion = await _chatbotBC.ObtenerConversacionConMensajes(
                    idConversacion,
                    CedulaUsuario);

                if (conversacion == null)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró la conversación."
                    });
                }

                var mensajes = conversacion.Mensajechatbots
                    .OrderBy(m => m.FechaEnvio)
                    .ThenBy(m => m.IdMensaje)
                    .Select(m => new
                    {
                        idMensaje = m.IdMensaje,
                        idTipoEmisor = m.IdTipoEmisor,
                        contenido = m.Contenido,
                        fechaEnvio = m.FechaEnvio
                    })
                    .ToList();

                return Ok(new
                {
                    idConversacion = conversacion.IdConversacion,
                    fechaInicio = conversacion.FechaInicio,
                    mensajes = mensajes
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "No se pudo completar la operación." });
            }

        }



        // Obtener conversaciones con el primer mensaje para generar el título
        [HttpGet("ObtenerConversaciones")]
        public async Task<IActionResult> ObtenerConversaciones()
        {
            try
            {
                var conversaciones = await _chatbotBC.ObtenerConversacionesUsuario(
                    CedulaUsuario);

                var resultado = conversaciones.Select(c =>
                {
                    var primerMensaje = c.Mensajechatbots
                        .OrderBy(m => m.FechaEnvio)
                        .ThenBy(m => m.IdMensaje)
                        .FirstOrDefault();

                    string titulo = primerMensaje?.Contenido ?? "Nueva conversación";

                    if (titulo.Length > 45)
                    {
                        titulo = titulo.Substring(0, 45) + "...";
                    }

                    return new
                    {
                        idConversacion = c.IdConversacion,
                        fechaInicio = c.FechaInicio,
                        fechaUltimaActividad = c.FechaUltimaActividad,
                        idEstadoActivo = c.IdEstadoActivo,
                        titulo = titulo
                    };
                }).ToList();

                return Ok(resultado);
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "No se pudo completar la operación." });
            }
        }

        // Eliminar una conversación
        [HttpDelete("EliminarConversacion/{idConversacion}")]
        public async Task<IActionResult> EliminarConversacion(
            int idConversacion)
        {
            if (idConversacion <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Los datos proporcionados no son válidos."
                });
            }

            try
            {
                bool eliminada = await _chatbotBC.EliminarConversacion(
                    idConversacion,
                    CedulaUsuario);

                if (!eliminada)
                {
                    return NotFound(new
                    {
                        mensaje = "No se encontró la conversación."
                    });
                }

                return Ok(new
                {
                    mensaje = "Conversación eliminada correctamente."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "No se pudo completar la operación."
                });
            }
        }


        public class EnviarMensajeChatbotRequest
        {
            public int IdConversacion { get; set; }
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
