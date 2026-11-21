using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Telefono
{
    public int IdTelefono { get; set; }

    public int Numero { get; set; }

    public int IdPrefijo { get; set; }

    public int IdTipoTelefono { get; set; }

    public virtual Prefijotelefonico IdPrefijoNavigation { get; set; } = null!;

    public virtual Tipotelefono IdTipoTelefonoNavigation { get; set; } = null!;

    public virtual ICollection<Empleado> CedulaEmpleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Usuario> CedulaUsuarios { get; set; } = new List<Usuario>();
}
