using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Estadoalertum
{
    public int IdEstadoAlerta { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Alertum> Alerta { get; set; } = new List<Alertum>();

    public virtual ICollection<Revisionalertum> Revisionalerta { get; set; } = new List<Revisionalertum>();
}
