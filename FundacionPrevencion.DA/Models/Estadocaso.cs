using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Estadocaso
{
    public int IdEstadoCaso { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Caso> Casos { get; set; } = new List<Caso>();
}
