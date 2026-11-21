using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Tiporecurso
{
    public int IdTipoRecurso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Recursoayudum> Recursoayuda { get; set; } = new List<Recursoayudum>();
}
