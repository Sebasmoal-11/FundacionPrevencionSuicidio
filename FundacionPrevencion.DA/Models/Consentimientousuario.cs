using System;
using System.Collections.Generic;

namespace Proyecto.Models;

public partial class Consentimientousuario
{
    public int IdConsentimiento { get; set; }

    public int CedulaUsuario { get; set; }

    public string VersionConsentimiento { get; set; } = null!;

    public DateTime FechaAceptacion { get; set; }

    public virtual Usuario CedulaUsuarioNavigation { get; set; } = null!;
}
