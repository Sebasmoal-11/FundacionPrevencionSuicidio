using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Empleado
{
    public int Cedula { get; set; }

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public DateTime FechaContratacion { get; set; }

    public int IdEstadoActivo { get; set; }

    public virtual ICollection<Asignacioncaso> Asignacioncasos { get; set; } = new List<Asignacioncaso>();

    public virtual ICollection<Auditoriaacceso> Auditoriaaccesos { get; set; } = new List<Auditoriaacceso>();

    public virtual Persona CedulaNavigation { get; set; } = null!;

    public virtual Estadoactivo IdEstadoActivoNavigation { get; set; } = null!;

    public virtual ICollection<Mensajeempleadoprofesional> Mensajeempleadoprofesionals { get; set; } = new List<Mensajeempleadoprofesional>();

    public virtual ICollection<Revisionalertum> Revisionalerta { get; set; } = new List<Revisionalertum>();

    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();

    public virtual ICollection<Telefono> IdTelefonos { get; set; } = new List<Telefono>();
}
