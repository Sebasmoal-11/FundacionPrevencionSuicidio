using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Conversacionchatbot
{
    public int IdConversacion { get; set; }

    public int CedulaUsuario { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaUltimaActividad { get; set; }

    public int IdEstadoActivo { get; set; }

    public virtual Usuario CedulaUsuarioNavigation { get; set; } = null!;

    public virtual Estadoactivo IdEstadoActivoNavigation { get; set; } = null!;

    public virtual ICollection<Mensajechatbot> Mensajechatbots { get; set; } = new List<Mensajechatbot>();

    public bool OcultaUsuario { get; set; }
}
