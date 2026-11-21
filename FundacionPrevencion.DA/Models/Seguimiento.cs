using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Seguimiento
{
    public int IdSeguimiento { get; set; }

    public int IdCaso { get; set; }

    public int CedulaEmpleado { get; set; }

    public DateTime FechaSeguimiento { get; set; }

    public string Observaciones { get; set; } = null!;

    public virtual Empleado CedulaEmpleadoNavigation { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;
}
