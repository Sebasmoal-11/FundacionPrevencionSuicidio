using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Recursoayudum
{
    public int IdRecurso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int IdTipoRecurso { get; set; }

    public int IdEstadoActivo { get; set; }

    public virtual Estadoactivo IdEstadoActivoNavigation { get; set; } = null!;

    public virtual Tiporecurso IdTipoRecursoNavigation { get; set; } = null!;

    public virtual ICollection<Recursocontacto> Recursocontactos { get; set; } = new List<Recursocontacto>();
}
