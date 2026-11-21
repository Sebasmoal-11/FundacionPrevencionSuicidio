using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Usuario
{
    public int Cedula { get; set; }

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public int IdEstadoActivo { get; set; }

    public virtual ICollection<Caso> Casos { get; set; } = new List<Caso>();

    public virtual Persona CedulaNavigation { get; set; } = null!;

    public virtual ICollection<Consentimientousuario> Consentimientousuarios { get; set; } = new List<Consentimientousuario>();

    public virtual ICollection<Conversacionchatbot> Conversacionchatbots { get; set; } = new List<Conversacionchatbot>();

    public virtual Estadoactivo IdEstadoActivoNavigation { get; set; } = null!;

    public virtual ICollection<Mensajeusuarioprofesional> Mensajeusuarioprofesionals { get; set; } = new List<Mensajeusuarioprofesional>();

    public virtual ICollection<Solicitudacompanamiento> Solicitudacompanamientos { get; set; } = new List<Solicitudacompanamiento>();

    public virtual ICollection<Telefono> IdTelefonos { get; set; } = new List<Telefono>();
}
