using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Nivelriesgo
{
    public int IdNivelRiesgo { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Resultadoanalisi> Resultadoanalisis { get; set; } = new List<Resultadoanalisi>();
}
