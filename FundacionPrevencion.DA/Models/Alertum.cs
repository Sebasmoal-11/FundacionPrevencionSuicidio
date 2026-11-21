using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Alertum
{
    public int IdAlerta { get; set; }

    public int IdAnalisis { get; set; }

    public int IdEstadoAlerta { get; set; }

    public DateTime FechaGeneracion { get; set; }

    public string Observaciones { get; set; } = null!;

    public virtual Resultadoanalisi IdAnalisisNavigation { get; set; } = null!;

    public virtual Estadoalertum IdEstadoAlertaNavigation { get; set; } = null!;

    public virtual ICollection<Revisionalertum> Revisionalerta { get; set; } = new List<Revisionalertum>();
}
