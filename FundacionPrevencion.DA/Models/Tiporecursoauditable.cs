using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Tiporecursoauditable
{
    public int IdTipoRecursoAuditable { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Auditoriaacceso> Auditoriaaccesos { get; set; } = new List<Auditoriaacceso>();
}
