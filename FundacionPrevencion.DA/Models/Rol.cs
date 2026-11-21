using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Rol
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Empleado> CedulaEmpleados { get; set; } = new List<Empleado>();
}
