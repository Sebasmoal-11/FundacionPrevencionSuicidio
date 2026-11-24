using System.ComponentModel.DataAnnotations;

namespace Proyecto.Autenticacion;

public sealed class IngresoForm
{
    [Required, EmailAddress, StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = "";

    [Required, StringLength(128), DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = "";
}

public sealed class RegistroForm
{
    [Required, RegularExpression("[0-9]{1,10}", ErrorMessage = "Ingrese una cédula numérica válida.")]
    [Display(Name = "Cédula")]
    public string Cedula { get; set; } = "";

    [Required, StringLength(80)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(80), Display(Name = "Primer apellido")]
    public string PrimerApellido { get; set; } = "";

    [Required, StringLength(80), Display(Name = "Segundo apellido")]
    public string SegundoApellido { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 12), DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contraseñas deben coincidir.")]
    [Display(Name = "Confirmar contraseña")]
    public string Confirmacion { get; set; } = "";
}
