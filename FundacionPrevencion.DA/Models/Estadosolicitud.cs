using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Estadosolicitud
{
    public int IdEstadoSolicitud { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Solicitudacompanamiento> Solicitudacompanamientos { get; set; } = new List<Solicitudacompanamiento>();
}
