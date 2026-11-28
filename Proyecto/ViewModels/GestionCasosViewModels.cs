namespace Proyecto.ViewModels
{
    public class GestionCasosViewModel
    {
        public List<GestionCasoFilaViewModel> Casos { get; set; } = new();
        public List<GestionCasoEstadoViewModel> Estados { get; set; } = new();
        public List<GestionCasoEmpleadoViewModel> EmpleadosAsignables { get; set; } = new();
        public List<GestionCasoAsignacionEstadoViewModel> EstadosAsignacion { get; set; } = new();
        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int Total { get; set; }
        public int TotalGeneral { get; set; }
        public int? EstadoSeleccionado { get; set; }
        public int? CedulaUsuario { get; set; }
        public List<GestionCasoAlertaViewModel> AlertasDisponibles { get; set; } = new();
    }

    public class GestionCasoFilaViewModel
    {
        public int IdCaso { get; set; }
        public string Codigo { get; set; } = "";
        public string FechaApertura { get; set; } = "";
        public int CedulaUsuario { get; set; }
        public string Usuario { get; set; } = "";
        public string Iniciales { get; set; } = "";
        public string CedulaParcial { get; set; } = "";
        public int IdEstadoCaso { get; set; }
        public string EstadoCaso { get; set; } = "";
        public string ClaseEstado { get; set; } = "";
        public bool DesdeAlerta { get; set; }
        public string Actualizacion { get; set; } = "";
    }

    public class GestionCasoEstadoViewModel
    {
        public int IdEstadoCaso { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Total { get; set; }
    }

    public class GestionCasoAlertaViewModel
    {
        public int IdAlerta { get; set; }
        public string Codigo { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
        public string NivelRiesgo { get; set; } = "";
        public int CedulaUsuario { get; set; }
        public string Usuario { get; set; } = "";
        public string Observaciones { get; set; } = "";
    }

    public class GestionCasoDetalleViewModel
    {
        public int IdCaso { get; set; }
        public string Codigo { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string FechaApertura { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Responsable { get; set; } = "";
        public string Descripcion { get; set; } = "";

        public bool DesdeAlerta { get; set; }
        public int? IdAlerta { get; set; }
        public string CodigoAlerta { get; set; } = "";
        public string EstadoAlerta { get; set; } = "";
        public string NivelRiesgo { get; set; } = "";
        public string ObservacionesAlerta { get; set; } = "";

        public List<GestionCasoAsignacionViewModel> HistorialAsignaciones { get; set; } = new();
    }

    public class GestionCasoAsignacionViewModel
    {
        public string Empleado { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
    }

    public class GestionCasoEmpleadoViewModel
    {
        public int Cedula { get; set; }
        public string Nombre { get; set; } = "";
    }

    public class GestionCasoAsignacionEstadoViewModel
    {
        public int IdEstadoAsignacion { get; set; }
        public string Nombre { get; set; } = "";
    }
}
