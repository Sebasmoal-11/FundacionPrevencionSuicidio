
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FundacionPrevencion.BW.Modelos
{
    public class AnalisisRiesgoGemini
    {
        [JsonPropertyName("haySenales")]
        public bool HaySenales { get; set; }

        [JsonPropertyName("nivelSugerido")]
        public string NivelSugerido { get; set; } = "Bajo";

        [JsonPropertyName("posiblePeligroInmediato")]
        public bool PosiblePeligroInmediato { get; set; }

        [JsonPropertyName("autolesion")]
        public bool Autolesion { get; set; }

        [JsonPropertyName("pensamientosSuicidas")]
        public bool PensamientosSuicidas { get; set; }

        [JsonPropertyName("violencia")]
        public bool Violencia { get; set; }

        [JsonPropertyName("abuso")]
        public bool Abuso { get; set; }

        [JsonPropertyName("malestarEmocional")]
        public bool MalestarEmocional { get; set; }

        [JsonPropertyName("senalesDetectadas")]
        public List<string> SenalesDetectadas { get; set; } = new();

        [JsonPropertyName("justificacion")]
        public string Justificacion { get; set; } = string.Empty;
    }
}
