using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Prefijotelefonico
{
    public int IdPrefijo { get; set; }

    public short Prefijo { get; set; }

    public string Pais { get; set; } = null!;

    public virtual ICollection<Telefono> Telefonos { get; set; } = new List<Telefono>();
}
