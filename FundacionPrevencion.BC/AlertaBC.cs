using FundacionPrevencion.DA;
using Proyecto.Models;

namespace FundacionPrevencion.BC
{
    public class AlertaBC
    {
        public const string Pendiente = "Pendiente de revisión";
        public const string EnRevision = "En revisión";
        public const string Cerrada = "Cerrada";

        // Reglas de transición entre los tres estados permitidos.
        private static readonly Dictionary<string, string[]> Transiciones = new()
        {
            [Pendiente] = new[] { EnRevision, Cerrada },
            [EnRevision] = new[] { Cerrada },
            [Cerrada] = Array.Empty<string>()
        };

        private readonly AlertaDA _alertaDA;

        public AlertaBC(AlertaDA alertaDA)
        {
            _alertaDA = alertaDA;
        }

        public async Task<List<Alertum>> Listar(
            int? idEstado, int? cedulaUsuario, int pagina, int tamano)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 100) tamano = 20;

            return await _alertaDA.Listar(idEstado, cedulaUsuario, pagina, tamano);
        }

        public async Task<int> Contar(int? idEstado, int? cedulaUsuario)
        {
            return await _alertaDA.Contar(idEstado, cedulaUsuario);
        }

        public async Task<Alertum?> ObtenerDetalle(int idAlerta)
        {
            if (idAlerta <= 0) return null;

            return await _alertaDA.ObtenerDetalle(idAlerta);
        }

        public async Task<List<Estadoalertum>> ObtenerEstados()
        {
            return await _alertaDA.ListarEstados();
        }

        public async Task<Dictionary<int, string>> ObtenerNombresPersonas(
            IEnumerable<int> cedulas)
        {
            return await _alertaDA.NombresPersonas(cedulas);
        }

        // Estados a los que puede pasar la alerta según su estado actual.
        public IReadOnlyList<Estadoalertum> Destinos(
            string estadoActual, IEnumerable<Estadoalertum> estados)
        {
            if (!Transiciones.TryGetValue(estadoActual, out var permitidos))
                return Array.Empty<Estadoalertum>();

            return estados
                .Where(e => permitidos.Contains(e.Nombre))
                .OrderBy(e => e.IdEstadoAlerta)
                .ToList();
        }

        public async Task<string?> CambiarEstado(
            int idAlerta, int idEstadoNuevo, int cedulaEmpleado, string? observaciones)
        {
            if (idAlerta <= 0 || cedulaEmpleado <= 0)
                return "Los datos de la alerta no son válidos.";

            string texto = (observaciones ?? "").Trim();
            if (texto.Length < 5)
                return "Escriba una observación de al menos 5 caracteres.";
            if (texto.Length > 1000)
                return "La observación no puede superar los 1000 caracteres.";

            var actual = await _alertaDA.ObtenerEstado(idAlerta);
            if (actual is null)
                return "La alerta indicada no existe.";

            var estados = await _alertaDA.ListarEstados();
            var destino = estados.FirstOrDefault(e => e.IdEstadoAlerta == idEstadoNuevo);
            if (destino is null)
                return "El estado seleccionado no es válido.";

            if (!Transiciones.TryGetValue(actual.Value.Nombre, out var permitidos))
                return "El estado actual de la alerta no permite cambios.";

            if (!permitidos.Contains(destino.Nombre))
                return $"No se puede pasar de «{actual.Value.Nombre}» a «{destino.Nombre}».";

            bool aplicado = await _alertaDA.CambiarEstado(
                idAlerta,
                actual.Value.IdEstado,
                idEstadoNuevo,
                cedulaEmpleado,
                texto,
                DateTime.UtcNow);

            return aplicado
                ? null
                : "La alerta cambió de estado mientras se procesaba la solicitud. Recargue la página e intente de nuevo.";
        }
    }
}
