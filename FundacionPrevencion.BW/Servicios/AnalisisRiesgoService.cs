

using FundacionPrevencion.BW.Modelos;
using System.Text.Json;

namespace FundacionPrevencion.BW.Servicios
{
    public class AnalisisRiesgoService
    {
        private readonly GeminiService _geminiService;

        public AnalisisRiesgoService(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        public async Task<AnalisisRiesgoResultado> AnalizarMensaje(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                throw new ArgumentException(
                    "El mensaje no puede estar vacío.");
            }

            // Solicitar el análisis estructurado a Gemini.
            string respuesta = await _geminiService.AnalizarRiesgo(mensaje);

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var resultado = JsonSerializer.Deserialize<AnalisisRiesgoResultado>(
                respuesta, opciones);

            if (resultado == null)
            {
                throw new InvalidOperationException(
                    "No se pudo interpretar el análisis de riesgo.");
            }

            // Validar el nivel de riesgo recibido.
            if (resultado.IdNivelRiesgo < 1 ||
                resultado.IdNivelRiesgo > 3)
            {
                throw new InvalidOperationException(
                    "Gemini devolvió un nivel de riesgo no válido.");
            }

            // Un posible peligro inmediato requiere clasificación prioritaria.
            if (resultado.PosiblePeligroInmediato)
            {
                resultado.IdNivelRiesgo = 3;
            }

            // Establecer el nombre según el identificador validado.
            resultado.NivelRiesgo = resultado.IdNivelRiesgo switch
            {
                1 => "Bajo",
                2 => "Moderado",
                3 => "Alto",
                _ => throw new InvalidOperationException(
                    "Nivel de riesgo no válido.")
            };

            return resultado;
        }
    }
}

