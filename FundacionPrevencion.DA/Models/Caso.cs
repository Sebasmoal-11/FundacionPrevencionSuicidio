using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Caso
{
    public int IdCaso { get; set; }

    public int CedulaUsuario { get; set; }

    public DateTime FechaApertura { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdEstadoCaso { get; set; }

    public int IdEstadoActivo { get; set; }

    public virtual ICollection<Asignacioncaso> Asignacioncasos { get; set; } = new List<Asignacioncaso>();

    public virtual Usuario CedulaUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Conversacionprofesional> Conversacionprofesionals { get; set; } = new List<Conversacionprofesional>();

    public virtual Estadoactivo IdEstadoActivoNavigation { get; set; } = null!;

    public virtual Estadocaso IdEstadoCasoNavigation { get; set; } = null!;

    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();

    public virtual ICollection<Factorriesgo> IdFactorRiesgos { get; set; } = new List<Factorriesgo>();

    public virtual ICollection<Solicitudacompanamiento> IdSolicituds { get; set; } = new List<Solicitudacompanamiento>();
}
