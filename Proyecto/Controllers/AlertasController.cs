using System.Security.Claims;
using FundacionPrevencion.BC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto.ViewModels;

namespace Proyecto.Controllers;

[Authorize(Policy = "CuentaPersonal")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("personal/alertas")]
public class AlertasController(AlertaBC alertaBC) : Controller
{
    private const int TamanoPagina = 20;

    private int CedulaEmpleado =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("")]
    public async Task<IActionResult> Index(int? estado, int? cedula, int pagina = 1)
    {
        if (pagina < 1) pagina = 1;

        int total = await alertaBC.Contar(estado, cedula);
        int totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;

        var filas = await alertaBC.Listar(estado, cedula, pagina, TamanoPagina);
        var estados = await alertaBC.ObtenerEstados();

        var cedulas = filas.Select(CedulaUsuarioDe);
        var nombres = await alertaBC.ObtenerNombresPersonas(cedulas);

        var modelo = new AlertaListaViewModel
        {
            Alertas = filas.Select(a =>
            {
                int cedulaUsuario = CedulaUsuarioDe(a);
                return new AlertaFilaViewModel
                {
                    IdAlerta = a.IdAlerta,
                    FechaGeneracion = a.FechaGeneracion,
                    IdEstado = a.IdEstadoAlerta,
                    Estado = a.IdEstadoAlertaNavigation.Nombre,
                    NivelRiesgo = a.IdAnalisisNavigation.IdNivelRiesgoNavigation.Nombre,
                    CedulaUsuario = cedulaUsuario,
                    Usuario = NombrePersona(nombres, cedulaUsuario)
                };
            }).ToList(),
            Estados = estados,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            Total = total,
            EstadoSeleccionado = estado,
            Cedula = cedula
        };

        return View(modelo);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var alerta = await alertaBC.ObtenerDetalle(id);
        if (alerta is null)
            return NotFound();

        int cedulaUsuario = CedulaUsuarioDe(alerta);

        var revisiones = alerta.Revisionalerta
            .OrderByDescending(r => r.FechaRevision)
            .ThenByDescending(r => r.IdRevisionAlerta)
            .ToList();

        var nombres = await alertaBC.ObtenerNombresPersonas(
            revisiones.Select(r => r.CedulaEmpleado).Append(cedulaUsuario));

        var estados = await alertaBC.ObtenerEstados();

        return View(new AlertaDetalleViewModel
        {
            Alerta = alerta,
            UsuarioCedula = cedulaUsuario,
            UsuarioNombre = NombrePersona(nombres, cedulaUsuario),
            Revisiones = revisiones.Select(r => new AlertaRevisionViewModel
            {
                FechaRevision = r.FechaRevision,
                CedulaEmpleado = r.CedulaEmpleado,
                EmpleadoNombre = NombrePersona(nombres, r.CedulaEmpleado),
                Estado = r.IdEstadoAlertaNavigation.Nombre,
                Observaciones = r.Observaciones
            }).ToList(),
            Destinos = alertaBC.Destinos(
                alerta.IdEstadoAlertaNavigation.Nombre, estados).ToList()
        });
    }

    [HttpPost("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id, int idEstado, string? observaciones)
    {
        string? error = await alertaBC.CambiarEstado(
            id, idEstado, CedulaEmpleado, observaciones);

        if (error is null)
            TempData["Mensaje"] = "Estado de la alerta actualizado.";
        else
            TempData["Error"] = error;

        return RedirectToAction(nameof(Detalle), new { id });
    }

    private static int CedulaUsuarioDe(Proyecto.Models.Alertum alerta)
    {
        return alerta.IdAnalisisNavigation.IdMensajeNavigation
            .IdConversacionNavigation.CedulaUsuario;
    }

    private static string NombrePersona(
        IReadOnlyDictionary<int, string> nombres, int cedula)
    {
        return nombres.TryGetValue(cedula, out string? nombre)
            ? nombre
            : $"Usuario {cedula}";
    }
}
