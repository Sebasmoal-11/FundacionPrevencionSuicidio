using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Tipotelefono
{
    public int IdTipoTelefono { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Telefono> Telefonos { get; set; } = new List<Telefono>();
}
