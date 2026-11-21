using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Mensajeempleadoprofesional
{
    public int IdMensajeEmpleado { get; set; }

    public int IdConversacionProfesional { get; set; }

    public int CedulaEmpleado { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public virtual Empleado CedulaEmpleadoNavigation { get; set; } = null!;

    public virtual Conversacionprofesional IdConversacionProfesionalNavigation { get; set; } = null!;
}
