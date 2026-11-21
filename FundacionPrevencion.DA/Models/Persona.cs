using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Persona
{
    public int Cedula { get; set; }

    public string Nombre { get; set; } = null!;

    public string PrimerApellido { get; set; } = null!;

    public string SegundoApellido { get; set; } = null!;

    public virtual Empleado? Empleado { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
