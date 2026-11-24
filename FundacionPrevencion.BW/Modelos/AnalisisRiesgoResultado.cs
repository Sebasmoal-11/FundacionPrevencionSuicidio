namespace FundacionPrevencion.BW.Modelos
{
    public class AnalisisRiesgoResultado
    {
        public int IdNivelRiesgo { get; set; }

        public string NivelRiesgo { get; set; } = string.Empty;

        public string Resultado { get; set; } = string.Empty;

        public bool HaySenales { get; set; }

        public bool PosiblePeligroInmediato { get; set; }

        public List<string> SenalesDetectadas { get; set; } = new();
    }
}