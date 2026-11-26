using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace FundacionPrevencion.DA
{
    public class AlertaDA
    {
        private readonly ApplicationDbContext _context;

        public AlertaDA(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consulta base con los datos necesarios para listar y detallar alertas.
        private IQueryable<Alertum> Consulta()
        {
            return _context.Alerta
                .AsNoTracking()
                .Include(a => a.IdEstadoAlertaNavigation)
                .Include(a => a.IdAnalisisNavigation)
                    .ThenInclude(r => r.IdNivelRiesgoNavigation)
                .Include(a => a.IdAnalisisNavigation)
                    .ThenInclude(r => r.IdMensajeNavigation)
                    .ThenInclude(m => m.IdConversacionNavigation);
        }

        public async Task<List<Alertum>> Listar(
            int? idEstado, int? cedulaUsuario, int pagina, int tamano)
        {
            var consulta = Consulta();

            if (idEstado.HasValue)
                consulta = consulta.Where(a => a.IdEstadoAlerta == idEstado.Value);

            if (cedulaUsuario.HasValue)
                consulta = consulta.Where(a =>
                    a.IdAnalisisNavigation.IdMensajeNavigation
                        .IdConversacionNavigation.CedulaUsuario == cedulaUsuario.Value);

            return await consulta
                .OrderByDescending(a => a.FechaGeneracion)
                .ThenByDescending(a => a.IdAlerta)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<int> Contar(int? idEstado, int? cedulaUsuario)
        {
            var consulta = _context.Alerta.AsNoTracking();

            if (idEstado.HasValue)
                consulta = consulta.Where(a => a.IdEstadoAlerta == idEstado.Value);

            if (cedulaUsuario.HasValue)
                consulta = consulta.Where(a =>
                    a.IdAnalisisNavigation.IdMensajeNavigation
                        .IdConversacionNavigation.CedulaUsuario == cedulaUsuario.Value);

            return await consulta.CountAsync();
        }

        public async Task<Alertum?> ObtenerDetalle(int idAlerta)
        {
            return await Consulta()
                .AsSplitQuery()
                .Include(a => a.Revisionalerta)
                    .ThenInclude(r => r.IdEstadoAlertaNavigation)
                .FirstOrDefaultAsync(a => a.IdAlerta == idAlerta);
        }

        // Nombres de las personas asociadas a las alertas, sin cargar sus cuentas.
        public async Task<Dictionary<int, string>> NombresPersonas(
            IEnumerable<int> cedulas)
        {
            var ids = cedulas.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            return await _context.Personas
                .AsNoTracking()
                .Where(p => ids.Contains(p.Cedula))
                .ToDictionaryAsync(
                    p => p.Cedula,
                    p => (p.Nombre + " " + p.PrimerApellido + " " + p.SegundoApellido).Trim());
        }

        public async Task<List<Estadoalertum>> ListarEstados()
        {
            return await _context.Estadoalerta
                .AsNoTracking()
                .OrderBy(e => e.IdEstadoAlerta)
                .ToListAsync();
        }

        public async Task<(int IdEstado, string Nombre)?> ObtenerEstado(int idAlerta)
        {
            var fila = await _context.Alerta
                .AsNoTracking()
                .Where(a => a.IdAlerta == idAlerta)
                .Select(a => new
                {
                    a.IdEstadoAlerta,
                    a.IdEstadoAlertaNavigation.Nombre
                })
                .FirstOrDefaultAsync();

            return fila is null ? null : (fila.IdEstadoAlerta, fila.Nombre);
        }

        // Cambia el estado y registra la revisión en una sola transacción.
        // El estado esperado evita que dos personas pisen el mismo cambio.
        public async Task<bool> CambiarEstado(
            int idAlerta,
            int idEstadoEsperado,
            int idEstadoNuevo,
            int cedulaEmpleado,
            string observaciones,
            DateTime fechaUtc)
        {
            await using var transaccion = await _context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var estados = await _context.Database.SqlQuery<int>($"""
                SELECT id_estado_alerta AS [Value]
                FROM dbo.alerta WITH (UPDLOCK, HOLDLOCK)
                WHERE id_alerta = {idAlerta}
                """).ToListAsync();

            if (estados.Count == 0 || estados[0] != idEstadoEsperado)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            int siguiente = await _context.Database.SqlQuery<int>($"""
                SELECT COALESCE(MAX(id_revision_alerta), 0) + 1 AS [Value]
                FROM dbo.revisionalerta WITH (UPDLOCK, HOLDLOCK)
                """).SingleAsync();

            await _context.Database.ExecuteSqlAsync($"""
                INSERT dbo.revisionalerta
                    (id_revision_alerta, id_alerta, cedula_empleado,
                     id_estado_alerta, fecha_revision, observaciones)
                VALUES
                    ({siguiente}, {idAlerta}, {cedulaEmpleado},
                     {idEstadoNuevo}, {fechaUtc}, {observaciones})
                """);

            await _context.Database.ExecuteSqlAsync($"""
                UPDATE dbo.alerta
                SET id_estado_alerta = {idEstadoNuevo}
                WHERE id_alerta = {idAlerta}
                """);

            await transaccion.CommitAsync();
            return true;
        }
    }
}
