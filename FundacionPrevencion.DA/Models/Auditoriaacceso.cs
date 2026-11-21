using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Auditoriaacceso
{
    public int IdAuditoria { get; set; }

    public int CedulaEmpleado { get; set; }

    public int IdTipoRecursoAuditable { get; set; }

    public int IdRecursoAccedido { get; set; }

    public DateTime FechaAcceso { get; set; }

    public virtual Empleado CedulaEmpleadoNavigation { get; set; } = null!;

    public virtual Tiporecursoauditable IdTipoRecursoAuditableNavigation { get; set; } = null!;
}
