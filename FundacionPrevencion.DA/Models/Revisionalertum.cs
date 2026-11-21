using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Revisionalertum
{
    public int IdRevisionAlerta { get; set; }

    public int IdAlerta { get; set; }

    public int CedulaEmpleado { get; set; }

    public int IdEstadoAlerta { get; set; }

    public DateTime FechaRevision { get; set; }

    public string Observaciones { get; set; } = null!;

    public virtual Empleado CedulaEmpleadoNavigation { get; set; } = null!;

    public virtual Alertum IdAlertaNavigation { get; set; } = null!;

    public virtual Estadoalertum IdEstadoAlertaNavigation { get; set; } = null!;
}
