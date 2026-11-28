using FundacionPrevencion.DA;
using Proyecto.Models;

namespace FundacionPrevencion.BC
{
    public class CasoBC
    {
        public const string RolPersonal = "Personal";

        private const string EstadoActivo = "Activo";
        private const int DescripcionMinima = 5;
        private const int DescripcionMaxima = 8000;

        private readonly CasoDA _casoDA;

        public CasoBC(CasoDA casoDA)
        {
            _casoDA = casoDA;
        }

        public async Task<List<Caso>> Listar(
            int cedulaEmpleado,
            int? idEstado,
            int? cedulaUsuario,
            int pagina,
            int tamano)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 100) tamano = 20;

            return await _casoDA.Listar(
                cedulaEmpleado, idEstado, cedulaUsuario, pagina, tamano);
        }

        public async Task<int> Contar(
            int cedulaEmpleado,
            int? idEstado,
            int? cedulaUsuario)
        {
            return await _casoDA.Contar(cedulaEmpleado, idEstado, cedulaUsuario);
        }

        public async Task<(Caso? Caso, Alertum? AlertaOrigen)> ObtenerDetalle(
            int idCaso,
            int cedulaEmpleado)
        {
            if (idCaso <= 0 || cedulaEmpleado <= 0)
                return (null, null);

            return await _casoDA.ObtenerDetalle(idCaso, cedulaEmpleado);
        }

        public async Task<List<Estadocaso>> ObtenerEstadosCaso()
        {
            return await _casoDA.ListarEstadosCaso();
        }

        public async Task<List<Estadoasignacion>> ObtenerEstadosAsignacion()
        {
            return await _casoDA.ListarEstadosAsignacion();
        }

        // Solo empleados activos con acceso al área del personal.
        public async Task<List<Empleado>> ObtenerEmpleadosAsignables()
        {
            var empleados = await _casoDA.ListarEmpleadosActivos();

            return empleados
                .Where(e => e.IdRols.Any(r => r.Nombre == RolPersonal))
                .ToList();
        }

        public async Task<(Usuario? Usuario, string? Error)> ObtenerUsuario(int cedula)
        {
            if (cedula <= 0)
                return (null, "La cédula del usuario no es válida.");

            var usuario = await _casoDA.ObtenerUsuario(cedula);

            return usuario is null
                ? (null, "No se encontró un usuario con esa cédula.")
                : (usuario, null);
        }

        public async Task<(Alertum? Alerta, bool YaTieneCaso, string? Error)>
            ObtenerAlertaParaCaso(int idAlerta)
        {
            if (idAlerta <= 0)
                return (null, false, "La alerta indicada no es válida.");

            var alerta = await _casoDA.ObtenerAlerta(idAlerta);
            if (alerta is null)
                return (null, false, "La alerta indicada no existe.");

            bool yaTieneCaso = await _casoDA.AlertaTieneCaso(idAlerta);
            return (alerta, yaTieneCaso, null);
        }

        public async Task<(int IdCaso, string? Error)> Crear(
            int cedulaEmpleadoActual,
            CrearCasoDatos datos)
        {
            if (cedulaEmpleadoActual <= 0)
                return (0, "La sesión del empleado no es válida.");
            if (datos is null)
                return (0, "Los datos del caso no son válidos.");
            if (datos.IdAlerta.HasValue && datos.IdAlerta.Value <= 0)
                return (0, "La alerta indicada no es válida.");

            var estadosCaso = await _casoDA.ListarEstadosCaso();
            if (!estadosCaso.Any(e => e.IdEstadoCaso == datos.IdEstadoCaso))
                return (0, "El estado del caso seleccionado no es válido.");

            var estadosAsignacion = await _casoDA.ListarEstadosAsignacion();
            if (!estadosAsignacion.Any(
                    e => e.IdEstadoAsignacion == datos.IdEstadoAsignacion))
                return (0, "El estado de la asignación seleccionado no es válido.");

            var empleado = await _casoDA.ObtenerEmpleado(
                datos.CedulaEmpleadoAsignado);
            if (empleado is null
                || empleado.IdEstadoActivoNavigation.Nombre != EstadoActivo)
                return (0, "El empleado asignado no existe o no está activo.");
            if (!empleado.IdRols.Any(r => r.Nombre == RolPersonal))
                return (0, "El empleado asignado no tiene acceso al área del personal.");

            int cedulaUsuario = datos.CedulaUsuario;
            string descripcion = (datos.Descripcion ?? "").Trim();

            if (datos.IdAlerta.HasValue)
            {
                var alerta = await _casoDA.ObtenerAlerta(datos.IdAlerta.Value);
                if (alerta is null)
                    return (0, "La alerta indicada no existe.");
                if (await _casoDA.AlertaTieneCaso(datos.IdAlerta.Value))
                    return (0, "La alerta ya tiene un caso asociado.");

                int cedulaAlerta = alerta.IdAnalisisNavigation
                    .IdMensajeNavigation.IdConversacionNavigation.CedulaUsuario;

                if (datos.CedulaUsuario > 0
                    && datos.CedulaUsuario != cedulaAlerta)
                    return (0, "La cédula del usuario no coincide con la alerta.");

                cedulaUsuario = cedulaAlerta;

                if (descripcion.Length == 0)
                    descripcion = (alerta.Observaciones ?? "").Trim();
            }
            else
            {
                if (cedulaUsuario <= 0)
                    return (0, "La cédula del usuario no es válida.");
                if (!await _casoDA.ExisteUsuario(cedulaUsuario))
                    return (0, "No se encontró un usuario con esa cédula.");
            }

            if (descripcion.Length < DescripcionMinima)
                return (0, "Escriba una descripción de al menos 5 caracteres.");
            if (descripcion.Length > DescripcionMaxima)
                return (0, "La descripción no puede superar los 8000 caracteres.");

            int idCaso = await _casoDA.Crear(
                cedulaUsuario,
                descripcion,
                datos.IdEstadoCaso,
                datos.CedulaEmpleadoAsignado,
                datos.IdEstadoAsignacion,
                datos.IdAlerta,
                DateTime.Now);

            return idCaso > 0
                ? (idCaso, null)
                : (0, "No se pudo crear el caso. Verifique que la alerta no tenga ya un caso asociado.");
        }

        public async Task<string?> CambiarEstado(
            int idCaso,
            int idEstadoCaso,
            int cedulaEmpleado)
        {
            if (idCaso <= 0 || idEstadoCaso <= 0 || cedulaEmpleado <= 0)
                return "Los datos del caso no son válidos.";

            var estados = await _casoDA.ListarEstadosCaso();
            if (!estados.Any(e => e.IdEstadoCaso == idEstadoCaso))
                return "El estado seleccionado no es válido.";

            var actual = await _casoDA.ObtenerEstadoCaso(idCaso, cedulaEmpleado);
            if (actual is null)
                return "El caso no existe o no tiene acceso.";
            if (actual.Value == idEstadoCaso)
                return "El caso ya se encuentra en ese estado.";

            bool aplicado = await _casoDA.CambiarEstado(
                idCaso, idEstadoCaso, cedulaEmpleado);

            return aplicado
                ? null
                : "El caso no existe o no tiene acceso.";
        }

        public async Task<string?> Reasignar(
            int idCaso,
            int cedulaEmpleadoActual,
            int cedulaNuevoEmpleado,
            int idEstadoAsignacion)
        {
            if (idCaso <= 0 || cedulaEmpleadoActual <= 0 || cedulaNuevoEmpleado <= 0)
                return "Los datos de la asignación no son válidos.";

            var estados = await _casoDA.ListarEstadosAsignacion();
            if (!estados.Any(e => e.IdEstadoAsignacion == idEstadoAsignacion))
                return "El estado de la asignación seleccionado no es válido.";

            var empleado = await _casoDA.ObtenerEmpleado(cedulaNuevoEmpleado);
            if (empleado is null
                || empleado.IdEstadoActivoNavigation.Nombre != EstadoActivo)
                return "El empleado asignado no existe o no está activo.";
            if (!empleado.IdRols.Any(r => r.Nombre == RolPersonal))
                return "El empleado asignado no tiene acceso al área del personal.";

            var responsable = await _casoDA.ObtenerResponsable(idCaso);
            if (responsable is null || responsable.Value != cedulaEmpleadoActual)
                return "El caso no existe o no tiene acceso.";
            if (responsable.Value == cedulaNuevoEmpleado)
                return "El caso ya está asignado a ese empleado.";

            bool aplicado = await _casoDA.Reasignar(
                idCaso,
                cedulaEmpleadoActual,
                cedulaNuevoEmpleado,
                idEstadoAsignacion,
                DateTime.Now);

            return aplicado
                ? null
                : "El caso no existe o no tiene acceso.";
        }
    }

    public sealed class CrearCasoDatos
    {
        public int? IdAlerta { get; set; }

        public int CedulaUsuario { get; set; }

        public string Descripcion { get; set; } = "";

        public int IdEstadoCaso { get; set; }

        public int CedulaEmpleadoAsignado { get; set; }

        public int IdEstadoAsignacion { get; set; }
    }
}
