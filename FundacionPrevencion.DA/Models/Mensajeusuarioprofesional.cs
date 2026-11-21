using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Mensajeusuarioprofesional
{
    public int IdMensajeUsuario { get; set; }

    public int IdConversacionProfesional { get; set; }

    public int CedulaUsuario { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public virtual Usuario CedulaUsuarioNavigation { get; set; } = null!;

    public virtual Conversacionprofesional IdConversacionProfesionalNavigation { get; set; } = null!;
}
