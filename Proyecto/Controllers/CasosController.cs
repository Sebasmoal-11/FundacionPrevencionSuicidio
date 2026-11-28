using System.Security.Claims;
using FundacionPrevencion.BC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto.Models;
using Proyecto.ViewModels;

namespace Proyecto.Controllers;

[ApiController]
[Authorize(Policy = "CuentaPersonal")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("personal/casos")]
public class CasosController(CasoBC casoBC) : ControllerBase
{
    private const int TamanoPagina = 20;

    private int CedulaEmpleado =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("")]
    public async Task<IActionResult> Index(
        int? estado, int? cedulaUsuario, int pagina = 1)
    {
        if (pagina < 1) pagina = 1;

        int total = await casoBC.Contar(
            CedulaEmpleado, estado, cedulaUsuario);
        int totalPaginas = Math.Max(
            1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;

        var casos = await casoBC.Listar(
            CedulaEmpleado, estado, cedulaUsuario, pagina, TamanoPagina);

        return Ok(new CasoListaResponse
        {
            Casos = casos.Select(MapearFila).ToList(),
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            Total = total,
            EstadoSeleccionado = estado,
            CedulaUsuario = cedulaUsuario
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var (caso, alerta) = await casoBC.ObtenerDetalle(
            id, CedulaEmpleado);

        if (caso is null)
            return NotFound(new { mensaje = "No se encontró el caso." });

        return Ok(MapearDetalle(caso, alerta));
    }

    [HttpGet("catalogos")]
    public async Task<IActionResult> Catalogos()
    {
        var estadosCaso = await casoBC.ObtenerEstadosCaso();
        var estadosAsignacion = await casoBC.ObtenerEstadosAsignacion();
        var empleados = await casoBC.ObtenerEmpleadosAsignables();

        return Ok(new CasoCatalogoResponse
        {
            EstadosCaso = estadosCaso.Select(e => new CasoEstadoCasoResponse
            {
                IdEstadoCaso = e.IdEstadoCaso,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion
            }).ToList(),
            EstadosAsignacion = estadosAsignacion
                .Select(e => new CasoEstadoAsignacionResponse
                {
                    IdEstadoAsignacion = e.IdEstadoAsignacion,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion
                }).ToList(),
            Empleados = empleados.Select(e => new CasoEmpleadoResponse
            {
                Cedula = e.Cedula,
                Nombre = NombreEmpleado(e)
            }).ToList()
        });
    }

    [HttpGet("usuarios/{cedula:int}")]
    public async Task<IActionResult> Usuario(int cedula)
    {
        var (usuario, error) = await casoBC.ObtenerUsuario(cedula);
        if (usuario is null)
            return NotFound(new { mensaje = error });

        return Ok(new CasoUsuarioResponse
        {
            Cedula = usuario.Cedula,
            Nombre = NombrePersona(usuario.CedulaNavigation)
        });
    }

    [HttpGet("alertas/{idAlerta:int}")]
    public async Task<IActionResult> Alerta(int idAlerta)
    {
        var (alerta, yaTieneCaso, error) =
            await casoBC.ObtenerAlertaParaCaso(idAlerta);

        if (alerta is null)
            return NotFound(new { mensaje = error });

        var conversacion = alerta.IdAnalisisNavigation
            .IdMensajeNavigation.IdConversacionNavigation;

        return Ok(new CasoAlertaPrefillResponse
        {
            IdAlerta = alerta.IdAlerta,
            CedulaUsuario = conversacion.CedulaUsuario,
            Usuario = NombrePersona(
                conversacion.CedulaUsuarioNavigation.CedulaNavigation),
            Observaciones = alerta.Observaciones,
            FechaGeneracion = alerta.FechaGeneracion,
            Estado = alerta.IdEstadoAlertaNavigation.Nombre,
            NivelRiesgo = alerta.IdAnalisisNavigation
                .IdNivelRiesgoNavigation.Nombre,
            YaTieneCaso = yaTieneCaso
        });
    }

    [HttpPost("")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearCasoRequest solicitud)
    {
        if (solicitud is null)
            return BadRequest(new { mensaje = "Los datos del caso no son válidos." });

        try
        {
            var (idCaso, error) = await casoBC.Crear(
                CedulaEmpleado,
                new CrearCasoDatos
                {
                    IdAlerta = solicitud.IdAlerta,
                    CedulaUsuario = solicitud.CedulaUsuario,
                    Descripcion = solicitud.Descripcion,
                    IdEstadoCaso = solicitud.IdEstadoCaso,
                    CedulaEmpleadoAsignado = solicitud.CedulaEmpleadoAsignado,
                    IdEstadoAsignacion = solicitud.IdEstadoAsignacion
                });

            if (error is not null)
                return BadRequest(new { mensaje = error });

            return Ok(new
            {
                idCaso,
                mensaje = "Caso creado correctamente."
            });
        }
        catch (Exception)
        {
            return StatusCode(
                500, new { mensaje = "No se pudo completar la operación." });
        }
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id, [FromBody] CambiarEstadoCasoRequest solicitud)
    {
        if (solicitud is null)
            return BadRequest(new { mensaje = "Los datos del estado no son válidos." });

        try
        {
            string? error = await casoBC.CambiarEstado(
                id, solicitud.IdEstadoCaso, CedulaEmpleado);

            if (error is not null)
                return BadRequest(new { mensaje = error });

            return Ok(new { mensaje = "Estado del caso actualizado." });
        }
        catch (Exception)
        {
            return StatusCode(
                500, new { mensaje = "No se pudo completar la operación." });
        }
    }

    [HttpPost("{id:int}/asignaciones")]
    public async Task<IActionResult> Reasignar(
        int id, [FromBody] ReasignarCasoRequest solicitud)
    {
        if (solicitud is null)
            return BadRequest(new { mensaje = "Los datos de la asignación no son válidos." });

        try
        {
            string? error = await casoBC.Reasignar(
                id,
                CedulaEmpleado,
                solicitud.CedulaEmpleado,
                solicitud.IdEstadoAsignacion);

            if (error is not null)
                return BadRequest(new { mensaje = error });

            return Ok(new { mensaje = "Caso reasignado correctamente." });
        }
        catch (Exception)
        {
            return StatusCode(
                500, new { mensaje = "No se pudo completar la operación." });
        }
    }

    private static CasoFilaResponse MapearFila(Caso caso)
    {
        var actual = caso.Asignacioncasos
            .OrderByDescending(a => a.IdAsignacion)
            .FirstOrDefault();

        return new CasoFilaResponse
        {
            IdCaso = caso.IdCaso,
            FechaApertura = caso.FechaApertura,
            Descripcion = caso.Descripcion,
            IdEstadoCaso = caso.IdEstadoCaso,
            EstadoCaso = caso.IdEstadoCasoNavigation.Nombre,
            CedulaUsuario = caso.CedulaUsuario,
            Usuario = NombrePersona(
                caso.CedulaUsuarioNavigation?.CedulaNavigation),
            CedulaEmpleadoAsignado = actual?.CedulaEmpleado ?? 0,
            EmpleadoAsignado = actual is null
                ? ""
                : NombreEmpleado(actual.CedulaEmpleadoNavigation),
            FechaAsignacion = actual?.FechaAsignacion ?? default,
            IdEstadoAsignacion = actual?.IdEstadoAsignacion ?? 0,
            EstadoAsignacion = actual?.IdEstadoAsignacionNavigation?.Nombre ?? ""
        };
    }

    private static CasoDetalleResponse MapearDetalle(
        Caso caso, Alertum? alerta)
    {
        var asignaciones = caso.Asignacioncasos
            .OrderByDescending(a => a.IdAsignacion)
            .ToList();

        int? idActual = asignaciones.FirstOrDefault()?.IdAsignacion;

        return new CasoDetalleResponse
        {
            IdCaso = caso.IdCaso,
            FechaApertura = caso.FechaApertura,
            Descripcion = caso.Descripcion,
            IdEstadoCaso = caso.IdEstadoCaso,
            EstadoCaso = caso.IdEstadoCasoNavigation.Nombre,
            IdEstadoActivo = caso.IdEstadoActivo,
            EstadoActivo = caso.IdEstadoActivoNavigation.Nombre,
            Usuario = new CasoUsuarioResponse
            {
                Cedula = caso.CedulaUsuario,
                Nombre = NombrePersona(
                    caso.CedulaUsuarioNavigation?.CedulaNavigation)
            },
            Asignaciones = asignaciones.Select(a => new CasoAsignacionResponse
            {
                IdAsignacion = a.IdAsignacion,
                CedulaEmpleado = a.CedulaEmpleado,
                Empleado = NombreEmpleado(a.CedulaEmpleadoNavigation),
                FechaAsignacion = a.FechaAsignacion,
                IdEstadoAsignacion = a.IdEstadoAsignacion,
                EstadoAsignacion = a.IdEstadoAsignacionNavigation.Nombre,
                Actual = a.IdAsignacion == idActual
            }).ToList(),
            AlertaOrigen = alerta is null ? null : new CasoAlertaOrigenResponse
            {
                IdAlerta = alerta.IdAlerta,
                FechaGeneracion = alerta.FechaGeneracion,
                Estado = alerta.IdEstadoAlertaNavigation.Nombre,
                NivelRiesgo = alerta.IdAnalisisNavigation
                    .IdNivelRiesgoNavigation.Nombre,
                Observaciones = alerta.Observaciones
            }
        };
    }

    private static string NombreEmpleado(Empleado? empleado)
    {
        return NombrePersona(empleado?.CedulaNavigation);
    }

    private static string NombrePersona(Persona? persona)
    {
        if (persona is null)
            return "";

        return $"{persona.Nombre} {persona.PrimerApellido} {persona.SegundoApellido}"
            .Trim();
    }
}
