using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Resultadoanalisi
{
    public int IdAnalisis { get; set; }

    public int IdMensaje { get; set; }

    public int IdNivelRiesgo { get; set; }

    public string Resultado { get; set; } = null!;

    public DateTime FechaAnalisis { get; set; }

    public virtual ICollection<Alertum> Alerta { get; set; } = new List<Alertum>();

    public virtual Mensajechatbot IdMensajeNavigation { get; set; } = null!;

    public virtual Nivelriesgo IdNivelRiesgoNavigation { get; set; } = null!;
}
