using Microsoft.Extensions.Configuration;
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
            string? apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "No se encontró la API Key de Gemini.");

            if (historial == null || historial.Count == 0)
                throw new ArgumentException(
                    "El historial de mensajes está vacío.");

            var contenidos = historial.Select(m => new
            {
                role = m.IdTipoEmisor == 1 ? "user" : "model",
                parts = new[]
                {
            new { text = m.Contenido }
        }
            }).ToArray();

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
                       frustración o preocupación, responde con sensibilidad
                       y ofrece orientación apropiada.

                    4. No realices diagnósticos psicológicos ni médicos.

                    5. No minimices los problemas de la persona ni utilices
                       expresiones que puedan hacerla sentir culpable.

                    6. Si identificas expresiones relacionadas con
                       autolesiones, suicidio, violencia o peligro inmediato,
                       prioriza la seguridad de la persona y recomiéndale
                       buscar ayuda humana inmediata.

                    7. No inventes números telefónicos, instituciones,
                       direcciones ni recursos de ayuda.

                    8. No afirmes que has realizado un análisis de riesgo
                       definitivo. La clasificación de riesgo corresponde
                       al sistema y sus reglas de negocio.

                    9. Formula preguntas de seguimiento cuando necesites
                       comprender mejor la situación, sin presionar al usuario.

                    10. Mantén tus respuestas claras, breves y cercanas.
                    """
                }
            }
                },
                contents = contenidos
            };

            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";

            _httpClient.DefaultRequestHeaders.Remove("x-goog-api-key");
            _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

            HttpResponseMessage respuesta = null!;

            for (int intento = 1; intento <= 3; intento++)
            {
                respuesta = await _httpClient.PostAsJsonAsync(
                    url, solicitud);

                if ((int)respuesta.StatusCode != 503)
                    break;

                if (intento < 3)
                {
                    respuesta.Dispose();
                    await Task.Delay(intento * 2000);
                }
            }

            var contenido = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Error de Gemini ({(int)respuesta.StatusCode}): {contenido}");
            }

            using var documento = JsonDocument.Parse(contenido);

            return documento.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "No se recibió una respuesta.";
        }
    }
}
