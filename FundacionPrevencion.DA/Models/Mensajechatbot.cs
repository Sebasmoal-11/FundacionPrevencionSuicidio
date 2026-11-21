using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Mensajechatbot
{
    public int IdMensaje { get; set; }

    public int IdConversacion { get; set; }

    public int IdTipoEmisor { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public virtual Conversacionchatbot IdConversacionNavigation { get; set; } = null!;

    public virtual Tipoemisorchatbot IdTipoEmisorNavigation { get; set; } = null!;

    public virtual ICollection<Resultadoanalisi> Resultadoanalisis { get; set; } = new List<Resultadoanalisi>();
}
