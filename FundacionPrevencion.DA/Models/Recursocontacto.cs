using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Recursocontacto
{
    public int IdRecursoContacto { get; set; }

    public int IdRecurso { get; set; }

    public int IdTipoContacto { get; set; }

    public string Valor { get; set; } = null!;

    public virtual Recursoayudum IdRecursoNavigation { get; set; } = null!;

    public virtual Tipocontactorecurso IdTipoContactoNavigation { get; set; } = null!;
}
