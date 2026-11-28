using System.Globalization;
using System.Security.Claims;
using FundacionPrevencion.BC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto.Models;
using Proyecto.ViewModels;

namespace Proyecto.Controllers;

[Authorize(Policy = "CuentaPersonal")]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PersonalController(CasoBC casoBC, AlertaBC alertaBC) : Controller
{
    private const int TamanoPagina = 10;
    private const int TamanoPaginaAlertas = 100;
    private const string EstadoAsignacionActiva = "Activa";

    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-ES");

    private int CedulaEmpleado =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GestionCasos(
        int? estado, int? estadoFiltro, int? cedulaUsuario, int pagina = 1)
    {
        // Las tarjetas de estado envían "estado" (0 = todos); el selector
        // del formulario envía "estadoFiltro".
        int? filtroEstado = estado.HasValue
            ? (estado.Value == 0 ? null : estado)
            : estadoFiltro;

        if (pagina < 1) pagina = 1;

        int total = await casoBC.Contar(
            CedulaEmpleado, filtroEstado, cedulaUsuario);
        int totalPaginas = Math.Max(
            1, (int)Math.Ceiling(total / (double)TamanoPagina));
        if (pagina > totalPaginas) pagina = totalPaginas;

        var casos = await casoBC.Listar(
            CedulaEmpleado, filtroEstado, cedulaUsuario, pagina, TamanoPagina);

        var estados = new List<GestionCasoEstadoViewModel>();
        foreach (var estadoCaso in await casoBC.ObtenerEstadosCaso())
        {
            estados.Add(new GestionCasoEstadoViewModel
            {
                IdEstadoCaso = estadoCaso.IdEstadoCaso,
                Nombre = estadoCaso.Nombre,
                Descripcion = estadoCaso.Descripcion,
                Total = await casoBC.Contar(
                    CedulaEmpleado, estadoCaso.IdEstadoCaso, cedulaUsuario)
            });
        }

        var filas = new List<GestionCasoFilaViewModel>();
        foreach (var caso in casos)
        {
            var (_, alerta) = await casoBC.ObtenerDetalle(
                caso.IdCaso, CedulaEmpleado);

            filas.Add(MapearFila(caso, alerta is not null));
        }

        var empleadosAsignables = await casoBC.ObtenerEmpleadosAsignables();

        var estadosAsignacion = await casoBC.ObtenerEstadosAsignacion();

        return View(new GestionCasosViewModel
        {
            Casos = filas,
            Estados = estados,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            Total = total,
            TotalGeneral = await casoBC.Contar(CedulaEmpleado, null, cedulaUsuario),
            EstadoSeleccionado = filtroEstado,
            CedulaUsuario = cedulaUsuario,
            AlertasDisponibles = await ObtenerAlertasDisponibles(),
            EmpleadosAsignables = empleadosAsignables
            .Select(e => new GestionCasoEmpleadoViewModel
            {
                Cedula = e.Cedula,
                Nombre = $"{e.CedulaNavigation.Nombre} {e.CedulaNavigation.PrimerApellido} {e.CedulaNavigation.SegundoApellido}".Trim()
            })
            .ToList(),

            EstadosAsignacion = estadosAsignacion
            .Select(e => new GestionCasoAsignacionEstadoViewModel
            {
                IdEstadoAsignacion = e.IdEstadoAsignacion,
                Nombre = e.Nombre
            })
            .ToList(),
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearCaso(
        string? origen,
        int? idAlerta,
        int? idUsuario,
        int? cedulaUsuario,
        int? idEstadoCaso,
        string? descripcion,
        string? returnUrl)
    {
        bool desdeAlerta = string.Equals(
            origen, "alerta", StringComparison.OrdinalIgnoreCase);
        bool manual = string.Equals(
            origen, "manual", StringComparison.OrdinalIgnoreCase);

        if (!desdeAlerta && !manual)
            return VolverConError("Seleccione el origen del caso.", returnUrl);

        // Permite reabrir el modal en el mismo origen si la operación falla.
        TempData["OrigenNuevoCaso"] = desdeAlerta ? "alerta" : "manual";

        // El usuario debe provenir de la búsqueda y coincidir con la cédula visible.
        if (idUsuario is null || idUsuario <= 0
            || cedulaUsuario is null || cedulaUsuario != idUsuario)
            return VolverConError(
                "Busque y seleccione un usuario válido.", returnUrl);

        if (idEstadoCaso is null || idEstadoCaso <= 0)
            return VolverConError(
                "Seleccione el estado inicial del caso.", returnUrl);

        if (desdeAlerta && (idAlerta is null || idAlerta <= 0))
            return VolverConError(
                "Seleccione una alerta de origen.", returnUrl);

        var estadosAsignacion = await casoBC.ObtenerEstadosAsignacion();
        var estadoActiva = estadosAsignacion.FirstOrDefault(e =>
            string.Equals(
                e.Nombre, EstadoAsignacionActiva, StringComparison.OrdinalIgnoreCase));

        if (estadoActiva is null)
            return VolverConError(
                "No se encontró el estado de asignación inicial.", returnUrl);

        try
        {
            var (idCaso, error) = await casoBC.Crear(
                CedulaEmpleado,
                new CrearCasoDatos
                {
                    IdAlerta = desdeAlerta ? idAlerta : null,
                    CedulaUsuario = idUsuario.Value,
                    Descripcion = descripcion ?? "",
                    IdEstadoCaso = idEstadoCaso.Value,
                    CedulaEmpleadoAsignado = CedulaEmpleado,
                    IdEstadoAsignacion = estadoActiva.IdEstadoAsignacion
                });

            if (error is not null)
                return VolverConError(error, returnUrl);

            TempData["Mensaje"] = $"Caso CAS-{idCaso} creado correctamente.";

            // Se vuelve a la primera página sin filtros para que el caso
            // recién creado (el más reciente) quede visible.
            return RedirectToAction(nameof(GestionCasos));
        }
        catch (Exception)
        {
            return VolverConError(
                "No se pudo completar la operación.", returnUrl);
        }
    }

    private IActionResult VolverConError(string mensaje, string? returnUrl)
    {
        TempData["Error"] = mensaje;
        TempData["AbrirModalNuevo"] = true;

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(GestionCasos));
    }

    private async Task<List<GestionCasoAlertaViewModel>> ObtenerAlertasDisponibles()
    {
        var disponibles = new List<GestionCasoAlertaViewModel>();

        int total = await alertaBC.Contar(null, null);
        if (total == 0)
            return disponibles;

        int paginas = (int)Math.Ceiling(
            total / (double)TamanoPaginaAlertas);

        var alertas = new List<Alertum>();
        for (int pagina = 1; pagina <= paginas; pagina++)
        {
            alertas.AddRange(await alertaBC.Listar(
                null, null, pagina, TamanoPaginaAlertas));
        }

        var cedulas = alertas
            .Select(CedulaUsuarioDeAlerta)
            .Distinct()
            .ToList();
        var nombres = await alertaBC.ObtenerNombresPersonas(cedulas);

        foreach (var alerta in alertas)
        {
            var (_, yaTieneCaso, _) =
                await casoBC.ObtenerAlertaParaCaso(alerta.IdAlerta);
            if (yaTieneCaso)
                continue;

            int cedula = CedulaUsuarioDeAlerta(alerta);
            string nombre = nombres.TryGetValue(cedula, out string? encontrado)
                ? encontrado
                : $"Usuario {cedula}";

            disponibles.Add(new GestionCasoAlertaViewModel
            {
                IdAlerta = alerta.IdAlerta,
                Codigo = $"ALT-{alerta.IdAlerta}",
                Fecha = alerta.FechaGeneracion.ToString("dd MMM yyyy", Cultura),
                Estado = alerta.IdEstadoAlertaNavigation.Nombre,
                NivelRiesgo = alerta.IdAnalisisNavigation
                    .IdNivelRiesgoNavigation.Nombre,
                CedulaUsuario = cedula,
                Usuario = nombre,
                Observaciones = alerta.Observaciones
            });
        }

        return disponibles;
    }

    private static int CedulaUsuarioDeAlerta(Alertum alerta)
    {
        return alerta.IdAnalisisNavigation.IdMensajeNavigation
            .IdConversacionNavigation.CedulaUsuario;
    }

    private static GestionCasoFilaViewModel MapearFila(Caso caso, bool desdeAlerta)
    {
        var actual = caso.Asignacioncasos
            .OrderByDescending(a => a.IdAsignacion)
            .FirstOrDefault();

        var persona = caso.CedulaUsuarioNavigation?.CedulaNavigation;
        DateTime actualizacion = actual?.FechaAsignacion ?? caso.FechaApertura;

        return new GestionCasoFilaViewModel
        {
            IdCaso = caso.IdCaso,
            Codigo = $"CAS-{caso.IdCaso}",
            FechaApertura = caso.FechaApertura.ToString("dd MMM yyyy", Cultura),
            CedulaUsuario = caso.CedulaUsuario,
            Usuario = NombreAbreviado(persona),
            Iniciales = Iniciales(persona),
            CedulaParcial = EnmascararCedula(caso.CedulaUsuario),
            IdEstadoCaso = caso.IdEstadoCaso,
            EstadoCaso = caso.IdEstadoCasoNavigation.Nombre,
            ClaseEstado = ClaseEstado(caso.IdEstadoCasoNavigation.Nombre),
            DesdeAlerta = desdeAlerta,
            Actualizacion = FormatearFecha(actualizacion)
        };
    }

    private static string NombreAbreviado(Persona? persona)
    {
        if (persona is null)
            return "";

        string inicial = string.IsNullOrWhiteSpace(persona.Nombre)
            ? ""
            : $"{persona.Nombre.Trim()[0]}.";
        string apellido = string.IsNullOrWhiteSpace(persona.PrimerApellido)
            ? ""
            : $"{persona.PrimerApellido.Trim()[0]}.";

        return $"{inicial} {apellido}".Trim();
    }

    private static string Iniciales(Persona? persona)
    {
        if (persona is null)
            return "";

        return $"{PrimeraLetra(persona.Nombre)}{PrimeraLetra(persona.PrimerApellido)}"
            .ToUpperInvariant();
    }

    private static char PrimeraLetra(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? ' ' : texto.Trim()[0];
    }

    private static string EnmascararCedula(int cedula)
    {
        string texto = cedula.ToString(CultureInfo.InvariantCulture);

        return texto.Length <= 3
            ? texto
            : new string('•', texto.Length - 3) + texto[^3..];
    }

    private static string ClaseEstado(string nombre)
    {
        return nombre switch
        {
            "Abierto" => "estado-abierto",
            "En seguimiento" => "estado-seguimiento",
            "Cerrado" => "estado-cerrado",
            _ => "estado-cerrado"
        };
    }

    private static string FormatearFecha(DateTime fecha)
    {
        DateTime hoy = DateTime.Today;

        if (fecha.Date == hoy)
            return $"Hoy, {fecha:HH:mm}";
        if (fecha.Date == hoy.AddDays(-1))
            return $"Ayer, {fecha:HH:mm}";
        if (fecha.Year == hoy.Year)
            return fecha.ToString("dd MMM", Cultura);

        return fecha.ToString("dd MMM yyyy", Cultura);
    }

    [HttpPost]
    public async Task<IActionResult> CambiarEstadoCaso(
        int idCaso,
        int idEstadoCaso)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int cedulaEmpleado))
        {
            TempData["Error"] = "No se pudo identificar al empleado.";
            return RedirectToAction(nameof(GestionCasos));
        }

        string? error = await casoBC.CambiarEstado(
            idCaso,
            idEstadoCaso,
            cedulaEmpleado);

        if (error is null)
        {
            TempData["Mensaje"] = "El estado del caso se actualizó correctamente.";
        }
        else
        {
            TempData["Error"] = error;
        }

        return RedirectToAction(nameof(GestionCasos));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerDetalleCaso(int idCaso)
    {
        var (caso, alerta) = await casoBC.ObtenerDetalle(
            idCaso, CedulaEmpleado);

        if (caso is null)
            return NotFound(new { mensaje = "El caso no existe o no tiene acceso." });

        var asignaciones = caso.Asignacioncasos
            .OrderByDescending(a => a.IdAsignacion)
            .ToList();

        var actual = asignaciones.FirstOrDefault();

        var persona = caso.CedulaUsuarioNavigation?.CedulaNavigation;

        string nombreUsuario = persona is null
            ? "Usuario no disponible"
            : $"{persona.Nombre} {persona.PrimerApellido} {persona.SegundoApellido}".Trim();

        string nombreResponsable = actual?.CedulaEmpleadoNavigation?.CedulaNavigation is Persona empleado
            ? $"{empleado.Nombre} {empleado.PrimerApellido} {empleado.SegundoApellido}".Trim()
            : "No asignado";

        var detalle = new GestionCasoDetalleViewModel
        {
            IdCaso = caso.IdCaso,
            Codigo = $"CAS-{caso.IdCaso}",
            Usuario = nombreUsuario,
            Cedula = caso.CedulaUsuario.ToString(),
            FechaApertura = caso.FechaApertura.ToString("dd/MM/yyyy HH:mm"),
            Estado = caso.IdEstadoCasoNavigation.Nombre,
            Responsable = nombreResponsable,
            Descripcion = caso.Descripcion ?? "",

            DesdeAlerta = alerta is not null,
            IdAlerta = alerta?.IdAlerta,
            CodigoAlerta = alerta is null ? "" : $"ALT-{alerta.IdAlerta}",
            EstadoAlerta = alerta?.IdEstadoAlertaNavigation?.Nombre ?? "",
            NivelRiesgo = alerta?.IdAnalisisNavigation?.IdNivelRiesgoNavigation?.Nombre ?? "",
            ObservacionesAlerta = alerta?.Observaciones ?? "",

            HistorialAsignaciones = asignaciones
                .Select(a => new GestionCasoAsignacionViewModel
                {
                    Empleado = a.CedulaEmpleadoNavigation?.CedulaNavigation is Persona p
                        ? $"{p.Nombre} {p.PrimerApellido} {p.SegundoApellido}".Trim()
                        : $"Empleado {a.CedulaEmpleado}",

                    Fecha = a.FechaAsignacion.ToString("dd/MM/yyyy HH:mm"),

                    Estado = a.IdEstadoAsignacionNavigation?.Nombre ?? ""
                })
                .ToList()
        };

        return Json(detalle);
    }

    [HttpPost]
    public async Task<IActionResult> ReasignarCaso(
    int idCaso,
    int cedulaNuevoEmpleado)
    {
        var estados = await casoBC.ObtenerEstadosAsignacion();

        var estadoActivo = estados.FirstOrDefault(e =>
            string.Equals(
                e.Nombre,
                EstadoAsignacionActiva,
                StringComparison.OrdinalIgnoreCase));

        if (estadoActivo is null)
        {
            TempData["Error"] = "No se encontró el estado Activa de la asignación.";
            return RedirectToAction(nameof(GestionCasos));
        }

        string? error = await casoBC.Reasignar(
            idCaso,
            CedulaEmpleado,
            cedulaNuevoEmpleado,
            estadoActivo.IdEstadoAsignacion);

        if (error is null)
        {
            TempData["Mensaje"] = "El caso fue reasignado correctamente.";
        }
        else
        {
            TempData["Error"] = error;
        }

        return RedirectToAction(nameof(GestionCasos));
    }

}