using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Tipocontactorecurso
{
    public int IdTipoContacto { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Recursocontacto> Recursocontactos { get; set; } = new List<Recursocontacto>();
}
