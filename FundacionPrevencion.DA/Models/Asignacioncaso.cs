using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Asignacioncaso
{
    public int IdAsignacion { get; set; }

    public int IdCaso { get; set; }

    public int CedulaEmpleado { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public int IdEstadoAsignacion { get; set; }

    public virtual Empleado CedulaEmpleadoNavigation { get; set; } = null!;

    public virtual Caso IdCasoNavigation { get; set; } = null!;

    public virtual Estadoasignacion IdEstadoAsignacionNavigation { get; set; } = null!;
}
