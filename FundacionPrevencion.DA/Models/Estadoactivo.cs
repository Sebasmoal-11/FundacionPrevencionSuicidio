using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Estadoactivo
{
    public int IdEstadoActivo { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Caso> Casos { get; set; } = new List<Caso>();

    public virtual ICollection<Conversacionchatbot> Conversacionchatbots { get; set; } = new List<Conversacionchatbot>();

    public virtual ICollection<Conversacionprofesional> Conversacionprofesionals { get; set; } = new List<Conversacionprofesional>();

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Recursoayudum> Recursoayuda { get; set; } = new List<Recursoayudum>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
