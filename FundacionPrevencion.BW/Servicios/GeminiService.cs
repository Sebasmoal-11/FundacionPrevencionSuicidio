
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Proyecto.Models;

namespace FundacionPrevencion.BW.Servicios
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Se conserva para las pruebas individuales de Gemini.
        public async Task<string> EnviarMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException(
                    "El mensaje no puede estar vacío.");

            var historial = new List<Mensajechatbot>
            {
                new Mensajechatbot
                {
                    IdTipoEmisor = 1,
                    Contenido = mensaje
                }
            };

            return await EnviarMensaje(historial);
        }

        // Envía a Gemini el historial completo de la conversación.
        public async Task<string> EnviarMensaje(
            List<Mensajechatbot> historial)
        {
            string apiKey = ObtenerApiKey();

            if (historial == null || historial.Count == 0)
                throw new ArgumentException(
                    "El historial de mensajes está vacío.");

            var contenidos = historial
                .Where(m => !string.IsNullOrWhiteSpace(m.Contenido))
                .Select(m => new
                {
                    role = m.IdTipoEmisor == 1 ? "user" : "model",
                    parts = new[]
                    {
                        new { text = m.Contenido }
                    }
                })
                .ToArray();

            if (contenidos.Length == 0)
                throw new ArgumentException(
                    "No hay mensajes válidos en el historial.");

            var solicitud = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = """
                                Eres un asistente virtual especializado en prevención,
                                orientación y apoyo emocional.

                                Tu objetivo es brindar un espacio seguro para que las
                                personas puedan expresar sus preocupaciones y recibir
                                orientación.

                                Sigue estas instrucciones:

                                1. Responde siempre en español, con un lenguaje natural,
                                   respetuoso, claro y empático.

                                2. Escucha atentamente lo que expresa el usuario y
                                   demuestra comprensión sin juzgarlo.

                                3. Si la persona expresa tristeza, ansiedad, miedo,
                                   frustración, preocupación o malestar emocional,
                                   responde con sensibilidad y ofrece orientación apropiada.

                                4. No realices diagnósticos psicológicos ni médicos.

                                5. No minimices los problemas de la persona ni utilices
                                   expresiones que puedan hacerla sentir culpable.

                                6. Si identificas expresiones relacionadas con
                                   autolesiones, suicidio, violencia o peligro inmediato,
                                   prioriza la seguridad de la persona.

                                7. Si existe peligro actual o inminente, recomienda
                                   buscar ayuda humana inmediata. En Costa Rica,
                                   recomienda llamar al 9-1-1 y acudir a un servicio
                                   de emergencias. También recomienda contactar a una
                                   persona de confianza que pueda acompañarla.

                                8. Si la persona presenta malestar emocional o señales
                                   de riesgo que requieren seguimiento, recomienda
                                   solicitar apoyo en el apartado de atención profesional
                                   de la Fundación dentro de esta plataforma.

                                9. No inventes números telefónicos, instituciones,
                                   direcciones, enlaces ni recursos de ayuda.

                                10. No afirmes que has realizado un análisis de riesgo
                                    definitivo. La clasificación de riesgo corresponde
                                    al sistema y sus reglas de negocio.

                                11. Formula preguntas de seguimiento cuando necesites
                                    comprender mejor la situación, sin presionar al usuario.

                                12. Evita repetir varias veces la misma recomendación.
                                    Mantén las respuestas claras, breves y cercanas.

                                13. En una situación de peligro inmediato, la atención
                                    de emergencias tiene prioridad. El apartado de
                                    atención profesional de la plataforma no sustituye
                                    la ayuda de emergencia.
                                """
                        }
                    }
                },
                contents = contenidos
            };

            string url =
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";

            string contenidoRespuesta =
                await EnviarSolicitudGemini(url, apiKey, solicitud);

            return ExtraerTextoRespuesta(contenidoRespuesta);
        }

        // Analiza el mensaje y solicita una respuesta JSON.
        public async Task<string> AnalizarRiesgo(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException(
                    "El mensaje no puede estar vacío.");

            string apiKey = ObtenerApiKey();

            var solicitud = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = """
                                Eres un sistema de apoyo para identificar señales
                                relacionadas con posibles situaciones de riesgo.

                                Analiza exclusivamente el mensaje proporcionado.
                                No realices diagnósticos clínicos.
                                No inventes información ni señales que no estén presentes.
                                No confundas una negación, una cita o una referencia
                                histórica con una intención actual.

                                Devuelve exclusivamente un JSON válido con estas
                                propiedades:
                                idNivelRiesgo, nivelRiesgo, resultado, haySenales,
                                posiblePeligroInmediato y senalesDetectadas.

                                Clasificación:
                                1 = Bajo: no se identifican señales relevantes.
                                2 = Moderado: señales que requieren atención y seguimiento.
                                3 = Alto: señales importantes que requieren atención prioritaria.

                                posiblePeligroInmediato debe ser true únicamente cuando
                                el mensaje contenga indicios explícitos de peligro actual
                                o inminente.

                                No inventes información.

                                El resultado debe ser un objeto JSON, no debe incluir
                                explicaciones fuera del JSON ni bloques Markdown.
                                """
                        }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = mensaje }
                        }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            string url =
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";

            string contenidoRespuesta =
                await EnviarSolicitudGemini(url, apiKey, solicitud);

            return ExtraerTextoRespuesta(contenidoRespuesta);
        }

        // Obtiene la API Key desde la configuración.
        private string ObtenerApiKey()
        {
            string? apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "No se encontró la API Key de Gemini.");

            return apiKey;
        }

        // Envía la solicitud HTTP a Gemini y reintenta errores 503.
        private async Task<string> EnviarSolicitudGemini(
            string url,
            string apiKey,
            object solicitud)
        {
            for (int intento = 1; intento <= 3; intento++)
            {
                using var solicitudHttp = new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

                solicitudHttp.Headers.Add(
                    "x-goog-api-key",
                    apiKey);

                solicitudHttp.Content = JsonContent.Create(solicitud);

                using HttpResponseMessage respuesta =
                    await _httpClient.SendAsync(solicitudHttp);

                string contenido =
                    await respuesta.Content.ReadAsStringAsync();

                if (respuesta.IsSuccessStatusCode)
                    return contenido;

                if ((int)respuesta.StatusCode == 503 && intento < 3)
                {
                    await Task.Delay(intento * 2000);
                    continue;
                }

                throw new HttpRequestException(
                    $"Error de Gemini ({(int)respuesta.StatusCode}): {contenido}");
            }

            throw new HttpRequestException(
                "Gemini no respondió después de varios intentos.");
        }

        // Extrae el texto generado desde la respuesta de Gemini.
        private string ExtraerTextoRespuesta(string contenido)
        {
            using JsonDocument documento =
                JsonDocument.Parse(contenido);

            if (!documento.RootElement.TryGetProperty(
                    "candidates", out JsonElement candidatos)
                || candidatos.GetArrayLength() == 0)
            {
                throw new InvalidOperationException(
                    "Gemini no devolvió una respuesta válida.");
            }

            JsonElement partes = candidatos[0]
                .GetProperty("content")
                .GetProperty("parts");

            foreach (JsonElement parte in partes.EnumerateArray())
            {
                if (parte.TryGetProperty(
                        "text", out JsonElement texto))
                {
                    string? resultado = texto.GetString();

                    if (!string.IsNullOrWhiteSpace(resultado))
                        return resultado;
                }
            }

            throw new InvalidOperationException(
                "Gemini no devolvió texto en su respuesta.");
        }
    }
}
