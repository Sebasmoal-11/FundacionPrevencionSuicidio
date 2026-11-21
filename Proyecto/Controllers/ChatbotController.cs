
using FundacionPrevencion.BC;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Proyecto.Controllers
{
    [ApiController]
    [Route("Chatbot")]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatbotBC _chatbotBC;

        public ChatbotController(ChatbotBC chatbotBC)
        {
            _chatbotBC = chatbotBC;
        }

        // Prueba de conexión con Gemini
        [HttpPost("ProbarGemini")]
        public async Task<IActionResult> ProbarGemini(
            [FromBody] MensajeChatbotRequest solicitud)
        {
            if (solicitud == null ||
                string.IsNullOrWhiteSpace(solicitud.Mensaje))
            {
                return BadRequest(new
                {
                    mensaje = "Debes ingresar un mensaje."
                });
            }

            try
            {
                string respuesta = await _chatbotBC
                    .EnviarMensajeGemini(solicitud.Mensaje);

                return Ok(new
                {
                    respuesta = respuesta
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    mensaje = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new
                {
                    mensaje = ex.Message
                });
            }
        }

        // Iniciar una conversación
        [HttpPost("IniciarConversacion")]
        public async Task<IActionResult> IniciarConversacion(
            [FromBody] IniciarConversacionRequest solicitud)
        {
            if (solicitud == null || solicitud.CedulaUsuario <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Debes proporcionar una cédula válida."
                });
            }

            try
            {
                var conversacion = await _chatbotBC.CrearConversacion(
                    solicitud.CedulaUsuario);

                return Ok(new
                {
                    mensaje = "Conversación iniciada correctamente.",
                    idConversacion = conversacion.IdConversacion,
                    cedulaUsuario = conversacion.CedulaUsuario,
                    fechaInicio = conversacion.FechaInicio
                });
            }
            catch (Exception ex)
            {
                //  TEMPORAL: Eliminar el detalle interno al terminar las pruebas
                return StatusCode(500, new
                {
                    mensaje = ex.Message,
                    detalle = ex.InnerException?.Message
                });
            }
        }

        // Enviar mensaje al chatbot
        [HttpPost("EnviarMensaje")]
        public async Task<IActionResult> EnviarMensaje(
            [FromBody] EnviarMensajeChatbotRequest solicitud)
        {
            if (solicitud == null ||
                solicitud.CedulaUsuario <= 0 ||
                solicitud.IdConversacion <= 0 ||
                string.IsNullOrWhiteSpace(solicitud.Mensaje))
            {
                return BadRequest(new
                {
                    mensaje = "Debes proporcionar una cédula, una conversación y un mensaje válido."
                });
            }

            try
            {
                string respuesta = await _chatbotBC.ProcesarMensaje(
                    solicitud.IdConversacion,
                    solicitud.CedulaUsuario,
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
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // Obtener mensajes de una conversación
        [HttpGet("ObtenerMensajes/{idConversacion}/{cedulaUsuario}")]
        public async Task<IActionResult> ObtenerMensajes(
            int idConversacion,
            int cedulaUsuario)
        {
            if (idConversacion <= 0 || cedulaUsuario <= 0)
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
                    cedulaUsuario);

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
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }

        }



        // Obtener conversaciones con el primer mensaje para generar el título
        [HttpGet("ObtenerConversaciones/{cedulaUsuario}")]
        public async Task<IActionResult> ObtenerConversaciones(int cedulaUsuario)
        {
            if (cedulaUsuario <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "Debes proporcionar una cédula válida."
                });
            }

            try
            {
                var conversaciones = await _chatbotBC.ObtenerConversacionesUsuario(
                    cedulaUsuario);

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
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // Eliminar una conversación
        [HttpDelete("EliminarConversacion/{idConversacion}/{cedulaUsuario}")]
        public async Task<IActionResult> EliminarConversacion(
            int idConversacion,
            int cedulaUsuario)
        {
            if (idConversacion <= 0 || cedulaUsuario <= 0)
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
                    cedulaUsuario);

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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = ex.Message
                });
            }
        }


        public class MensajeChatbotRequest
        {
            public string Mensaje { get; set; } = string.Empty;
        }

        public class IniciarConversacionRequest
        {
            public int CedulaUsuario { get; set; }
        }

        public class EnviarMensajeChatbotRequest
        {
            public int CedulaUsuario { get; set; }
            public int IdConversacion { get; set; }
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
//Agregue esto para probar la IA cuando unamos los modulos hay que conectarlo a el inicio de sesion 

