using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Factorriesgo
{
    public int IdFactorRiesgo { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Caso> IdCasos { get; set; } = new List<Caso>();
}
