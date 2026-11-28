namespace Proyecto.Models
{
    public partial class Alertacaso
    {
        public int IdAlertaCaso { get; set; }

        public int IdAlerta { get; set; }

        public int IdCaso { get; set; }

        public virtual Alertum IdAlertaNavigation { get; set; } = null!;

        public virtual Caso IdCasoNavigation { get; set; } = null!;
    }
}
