using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Tipoemisorchatbot
{
    public int IdTipoEmisor { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Mensajechatbot> Mensajechatbots { get; set; } = new List<Mensajechatbot>();
}
