using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Solicitudacompanamiento
{
    public int IdSolicitud { get; set; }

    public int CedulaUsuario { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdEstadoSolicitud { get; set; }

    public virtual Usuario CedulaUsuarioNavigation { get; set; } = null!;

    public virtual Estadosolicitud IdEstadoSolicitudNavigation { get; set; } = null!;

    public virtual ICollection<Caso> IdCasos { get; set; } = new List<Caso>();
}
