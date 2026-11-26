using Proyecto.Models;

namespace Proyecto.ViewModels
{
    public class AlertaListaViewModel
    {
        public List<AlertaFilaViewModel> Alertas { get; set; } = new();
        public List<Estadoalertum> Estados { get; set; } = new();
        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int Total { get; set; }
        public int? EstadoSeleccionado { get; set; }
        public int? Cedula { get; set; }
    }

    public class AlertaFilaViewModel
    {
        public int IdAlerta { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public int IdEstado { get; set; }
        public string Estado { get; set; } = "";
        public string NivelRiesgo { get; set; } = "";
        public int CedulaUsuario { get; set; }
        public string Usuario { get; set; } = "";
    }

    public class AlertaDetalleViewModel
    {
        public Alertum Alerta { get; set; } = null!;
        public int UsuarioCedula { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public List<AlertaRevisionViewModel> Revisiones { get; set; } = new();
        public List<Estadoalertum> Destinos { get; set; } = new();
    }

    public class AlertaRevisionViewModel
    {
        public DateTime FechaRevision { get; set; }
        public int CedulaEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Observaciones { get; set; } = "";
    }
}
