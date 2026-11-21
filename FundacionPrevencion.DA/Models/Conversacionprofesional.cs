using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Conversacionprofesional
{
    public int IdConversacionProfesional { get; set; }

    public int IdCaso { get; set; }

    public DateTime FechaInicio { get; set; }

    public int IdEstadoActivo { get; set; }

    public virtual Caso IdCasoNavigation { get; set; } = null!;

    public virtual Estadoactivo IdEstadoActivoNavigation { get; set; } = null!;

    public virtual ICollection<Mensajeempleadoprofesional> Mensajeempleadoprofesionals { get; set; } = new List<Mensajeempleadoprofesional>();

    public virtual ICollection<Mensajeusuarioprofesional> Mensajeusuarioprofesionals { get; set; } = new List<Mensajeusuarioprofesional>();
}
