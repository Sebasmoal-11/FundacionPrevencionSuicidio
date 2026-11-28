namespace Proyecto.ViewModels;

public class CasoListaResponse
{
    public List<CasoFilaResponse> Casos { get; set; } = new();
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int Total { get; set; }
    public int? EstadoSeleccionado { get; set; }
    public int? CedulaUsuario { get; set; }
}

public class CasoFilaResponse
{
    public int IdCaso { get; set; }
    public DateTime FechaApertura { get; set; }
    public string Descripcion { get; set; } = "";
    public int IdEstadoCaso { get; set; }
    public string EstadoCaso { get; set; } = "";
    public int CedulaUsuario { get; set; }
    public string Usuario { get; set; } = "";
    public int CedulaEmpleadoAsignado { get; set; }
    public string EmpleadoAsignado { get; set; } = "";
    public DateTime FechaAsignacion { get; set; }
    public int IdEstadoAsignacion { get; set; }
    public string EstadoAsignacion { get; set; } = "";
}

public class CasoDetalleResponse
{
    public int IdCaso { get; set; }
    public DateTime FechaApertura { get; set; }
    public string Descripcion { get; set; } = "";
    public int IdEstadoCaso { get; set; }
    public string EstadoCaso { get; set; } = "";
    public int IdEstadoActivo { get; set; }
    public string EstadoActivo { get; set; } = "";
    public CasoUsuarioResponse Usuario { get; set; } = new();
    public List<CasoAsignacionResponse> Asignaciones { get; set; } = new();
    public CasoAlertaOrigenResponse? AlertaOrigen { get; set; }
}

public class CasoAsignacionResponse
{
    public int IdAsignacion { get; set; }
    public int CedulaEmpleado { get; set; }
    public string Empleado { get; set; } = "";
    public DateTime FechaAsignacion { get; set; }
    public int IdEstadoAsignacion { get; set; }
    public string EstadoAsignacion { get; set; } = "";
    public bool Actual { get; set; }
}

public class CasoAlertaOrigenResponse
{
    public int IdAlerta { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string Estado { get; set; } = "";
    public string NivelRiesgo { get; set; } = "";
    public string Observaciones { get; set; } = "";
}

public class CasoCatalogoResponse
{
    public List<CasoEstadoCasoResponse> EstadosCaso { get; set; } = new();
    public List<CasoEstadoAsignacionResponse> EstadosAsignacion { get; set; } = new();
    public List<CasoEmpleadoResponse> Empleados { get; set; } = new();
}

public class CasoEstadoCasoResponse
{
    public int IdEstadoCaso { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
}

public class CasoEstadoAsignacionResponse
{
    public int IdEstadoAsignacion { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
}

public class CasoEmpleadoResponse
{
    public int Cedula { get; set; }
    public string Nombre { get; set; } = "";
}

public class CasoUsuarioResponse
{
    public int Cedula { get; set; }
    public string Nombre { get; set; } = "";
}

public class CasoAlertaPrefillResponse
{
    public int IdAlerta { get; set; }
    public int CedulaUsuario { get; set; }
    public string Usuario { get; set; } = "";
    public string Observaciones { get; set; } = "";
    public DateTime FechaGeneracion { get; set; }
    public string Estado { get; set; } = "";
    public string NivelRiesgo { get; set; } = "";
    public bool YaTieneCaso { get; set; }
}

public class CrearCasoRequest
{
    public int? IdAlerta { get; set; }
    public int CedulaUsuario { get; set; }
    public string Descripcion { get; set; } = "";
    public int IdEstadoCaso { get; set; }
    public int CedulaEmpleadoAsignado { get; set; }
    public int IdEstadoAsignacion { get; set; }
}

public class CambiarEstadoCasoRequest
{
    public int IdEstadoCaso { get; set; }
}

public class ReasignarCasoRequest
{
    public int CedulaEmpleado { get; set; }
    public int IdEstadoAsignacion { get; set; }
}
