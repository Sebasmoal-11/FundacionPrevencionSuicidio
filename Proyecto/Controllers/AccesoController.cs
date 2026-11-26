using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Proyecto.Autenticacion;

namespace Proyecto.Controllers;

[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AccesoController(Cuentas cuentas) : Controller
{
    [HttpGet("/registro")]
    public IActionResult Registro() => User.Identity?.IsAuthenticated == true
        ? Destino()
        : View(new RegistroForm());

    [HttpPost("/registro"), EnableRateLimiting("acceso")]
    public async Task<IActionResult> Registro(RegistroForm formulario)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Destino();

        if (!int.TryParse(formulario.Cedula, out int cedula) || cedula <= 0)
            ModelState.AddModelError(nameof(formulario.Cedula), "Ingrese una cédula numérica válida.");

        if (!ModelState.IsValid)
            return View(formulario);

        var error = await cuentas.Crear(formulario, cedula);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View(formulario);
        }

        TempData["Mensaje"] = "Cuenta creada. Ya puede iniciar sesión.";
        return RedirectToAction(nameof(Ingreso));
    }

    [HttpGet("/"), HttpGet("/ingreso")]
    public IActionResult Ingreso() => VistaIngreso(false, new IngresoForm());

    [HttpGet("/personal/ingreso")]
    public IActionResult IngresoPersonal() => VistaIngreso(true, new IngresoForm());

    [HttpPost("/ingreso"), EnableRateLimiting("acceso")]
    public Task<IActionResult> Ingreso(IngresoForm formulario) => Autenticar(formulario, false);

    [HttpPost("/personal/ingreso"), EnableRateLimiting("acceso")]
    public Task<IActionResult> IngresoPersonal(IngresoForm formulario) => Autenticar(formulario, true);

    [Authorize(Policy = "CuentaUsuario"), HttpGet("/cuenta")]
    public IActionResult Cuenta()
    {
        ViewData["Title"] = "Mi cuenta";
        return View();
    }

    [Authorize(Policy = "CuentaPersonal"), HttpGet("/personal")]
    public IActionResult Personal()
    {
        ViewData["Title"] = "Área del personal";
        return View("Panel");
    }

    [Authorize, HttpPost("/salir")]
    public async Task<IActionResult> Salir()
    {
        var empleado = User.IsInRole(Cuentas.Personal);
        await cuentas.Cerrar(User);
        await HttpContext.SignOutAsync();
        TempData["Mensaje"] = "Sesión cerrada.";
        return RedirectToAction(empleado ? nameof(IngresoPersonal) : nameof(Ingreso));
    }

    [HttpGet("/acceso-denegado")]
    public IActionResult Denegado()
    {
        Response.StatusCode = 403;
        return View();
    }

    private IActionResult Destino() => RedirectToAction(User.IsInRole(Cuentas.Personal)
        ? nameof(Personal)
        : nameof(Cuenta));

    private IActionResult VistaIngreso(bool empleado, IngresoForm formulario)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Destino();

        ViewData["Empleado"] = empleado;
        return View("Ingreso", formulario);
    }

    private async Task<IActionResult> Autenticar(IngresoForm formulario, bool empleado)
    {
        if (User.Identity?.IsAuthenticated == true || !ModelState.IsValid)
            return VistaIngreso(empleado, formulario);

        var vence = DateTimeOffset.UtcNow.AddHours(8);
        var (cuenta, error) = await cuentas.Iniciar(formulario, empleado, vence);
        if (cuenta is null)
        {
            ModelState.AddModelError("", error!);
            return VistaIngreso(empleado, formulario);
        }

        await HttpContext.SignInAsync(cuenta, new AuthenticationProperties { ExpiresUtc = vence });
        TempData["Mensaje"] = "Sesión iniciada.";
        return RedirectToAction(empleado ? nameof(Personal) : nameof(Cuenta));
    }
}
