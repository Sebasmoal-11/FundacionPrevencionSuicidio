using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Estadoasignacion
{
    public int IdEstadoAsignacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Asignacioncaso> Asignacioncasos { get; set; } = new List<Asignacioncaso>();
}
