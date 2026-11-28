using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;

namespace FundacionPrevencion.DA
{
    public class CasoDA
    {
        private readonly ApplicationDbContext _context;

        public CasoDA(ApplicationDbContext context)
        {
            _context = context;
        }

        // Consulta base con los datos visibles del caso.
        private static IQueryable<Caso> ConDatos(IQueryable<Caso> consulta)
        {
            return consulta
                .Include(c => c.IdEstadoCasoNavigation)
                .Include(c => c.IdEstadoActivoNavigation)
                .Include(c => c.CedulaUsuarioNavigation)
                    .ThenInclude(u => u.CedulaNavigation)
                .Include(c => c.Asignacioncasos)
                    .ThenInclude(a => a.CedulaEmpleadoNavigation)
                    .ThenInclude(e => e.CedulaNavigation)
                .Include(c => c.Asignacioncasos)
                    .ThenInclude(a => a.IdEstadoAsignacionNavigation);
        }

        // Casos cuyo responsable vigente es el empleado indicado.
        // La asignación vigente es la de mayor id_asignacion.
        private IQueryable<Caso> ConsultaAsignados(int cedulaEmpleado)
        {
            var ultimas = _context.Asignacioncasos
                .GroupBy(a => a.IdCaso)
                .Select(g => g.Max(a => a.IdAsignacion));

            var ids = _context.Asignacioncasos
                .Where(a => ultimas.Contains(a.IdAsignacion)
                    && a.CedulaEmpleado == cedulaEmpleado)
                .Select(a => a.IdCaso);

            return _context.Casos
                .AsNoTracking()
                .Where(c => ids.Contains(c.IdCaso));
        }

        // Bloquea la fila del caso para fijar un orden de bloqueo común
        // (caso -> asignacioncaso) y evitar interbloqueos con la creación.
        private async Task<bool> BloquearCaso(int idCaso)
        {
            int existe = await _context.Database.SqlQuery<int>($"""
                SELECT 1 AS [Value]
                FROM dbo.caso WITH (UPDLOCK, HOLDLOCK)
                WHERE id_caso = {idCaso}
                """).FirstOrDefaultAsync();

            return existe == 1;
        }

        public async Task<List<Caso>> Listar(
            int cedulaEmpleado,
            int? idEstado,
            int? cedulaUsuario,
            int pagina,
            int tamano)
        {
            var consulta = ConsultaAsignados(cedulaEmpleado);

            if (idEstado.HasValue)
                consulta = consulta.Where(c => c.IdEstadoCaso == idEstado.Value);

            if (cedulaUsuario.HasValue)
                consulta = consulta.Where(c => c.CedulaUsuario == cedulaUsuario.Value);

            return await ConDatos(consulta)
                .AsSplitQuery()
                .OrderByDescending(c => c.FechaApertura)
                .ThenByDescending(c => c.IdCaso)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
        }

        public async Task<int> Contar(
            int cedulaEmpleado,
            int? idEstado,
            int? cedulaUsuario)
        {
            var consulta = ConsultaAsignados(cedulaEmpleado);

            if (idEstado.HasValue)
                consulta = consulta.Where(c => c.IdEstadoCaso == idEstado.Value);

            if (cedulaUsuario.HasValue)
                consulta = consulta.Where(c => c.CedulaUsuario == cedulaUsuario.Value);

            return await consulta.CountAsync();
        }

        public async Task<(Caso? Caso, Alertum? AlertaOrigen)> ObtenerDetalle(
            int idCaso,
            int cedulaEmpleado)
        {
            var caso = await ConDatos(ConsultaAsignados(cedulaEmpleado))
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.IdCaso == idCaso);

            if (caso is null)
                return (null, null);

            return (caso, await ObtenerAlertaOrigen(idCaso));
        }

        public async Task<Alertum?> ObtenerAlertaOrigen(int idCaso)
        {
            var enlace = await _context.Alertacasos
                .AsNoTracking()
                .Include(x => x.IdAlertaNavigation)
                    .ThenInclude(a => a.IdEstadoAlertaNavigation)
                .Include(x => x.IdAlertaNavigation)
                    .ThenInclude(a => a.IdAnalisisNavigation)
                    .ThenInclude(r => r.IdNivelRiesgoNavigation)
                .FirstOrDefaultAsync(x => x.IdCaso == idCaso);

            return enlace?.IdAlertaNavigation;
        }

        public async Task<int?> ObtenerEstadoCaso(int idCaso, int cedulaEmpleado)
        {
            return await ConsultaAsignados(cedulaEmpleado)
                .Where(c => c.IdCaso == idCaso)
                .Select(c => (int?)c.IdEstadoCaso)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> ObtenerResponsable(int idCaso)
        {
            return await _context.Asignacioncasos
                .AsNoTracking()
                .Where(a => a.IdCaso == idCaso)
                .OrderByDescending(a => a.IdAsignacion)
                .Select(a => (int?)a.CedulaEmpleado)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Crear(
            int cedulaUsuario,
            string descripcion,
            int idEstadoCaso,
            int cedulaEmpleadoAsignado,
            int idEstadoAsignacion,
            int? idAlerta,
            DateTime fecha)
        {
            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                int idEstadoActivo = await _context.Estadoactivos
                    .Where(e => e.Nombre == "Activo")
                    .Select(e => e.IdEstadoActivo)
                    .SingleAsync();

                int idCaso = await _context.Database.SqlQuery<int>($"""
                    SELECT COALESCE(MAX(id_caso), 0) + 1 AS [Value]
                    FROM dbo.caso WITH (UPDLOCK, HOLDLOCK)
                    """).SingleAsync();

                _context.Casos.Add(new Caso
                {
                    IdCaso = idCaso,
                    CedulaUsuario = cedulaUsuario,
                    FechaApertura = fecha,
                    Descripcion = descripcion,
                    IdEstadoCaso = idEstadoCaso,
                    IdEstadoActivo = idEstadoActivo
                });

                int idAsignacion = await _context.Database.SqlQuery<int>($"""
                    SELECT COALESCE(MAX(id_asignacion), 0) + 1 AS [Value]
                    FROM dbo.asignacioncaso WITH (UPDLOCK, HOLDLOCK)
                    """).SingleAsync();

                _context.Asignacioncasos.Add(new Asignacioncaso
                {
                    IdAsignacion = idAsignacion,
                    IdCaso = idCaso,
                    CedulaEmpleado = cedulaEmpleadoAsignado,
                    FechaAsignacion = fecha,
                    IdEstadoAsignacion = idEstadoAsignacion
                });

                if (idAlerta.HasValue)
                {
                    int idAlertaCaso = await _context.Database.SqlQuery<int>($"""
                        SELECT COALESCE(MAX(id_alerta_caso), 0) + 1 AS [Value]
                        FROM dbo.alertacaso WITH (UPDLOCK, HOLDLOCK)
                        """).SingleAsync();

                    _context.Alertacasos.Add(new Alertacaso
                    {
                        IdAlertaCaso = idAlertaCaso,
                        IdAlerta = idAlerta.Value,
                        IdCaso = idCaso
                    });
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return idCaso;
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                await transaccion.RollbackAsync();
                return 0;
            }
        }

        public async Task<bool> CambiarEstado(
            int idCaso,
            int idEstadoCaso,
            int cedulaEmpleado)
        {
            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            if (!await BloquearCaso(idCaso))
            {
                await transaccion.RollbackAsync();
                return false;
            }

            var responsable = await _context.Database.SqlQuery<int>($"""
                SELECT TOP 1 cedula_empleado AS [Value]
                FROM dbo.asignacioncaso WITH (UPDLOCK, HOLDLOCK)
                WHERE id_caso = {idCaso}
                ORDER BY id_asignacion DESC
                """).FirstOrDefaultAsync();

            if (responsable != cedulaEmpleado)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            int afectados = await _context.Database.ExecuteSqlAsync($"""
                UPDATE dbo.caso
                SET id_estado_caso = {idEstadoCaso}
                WHERE id_caso = {idCaso}
                """);

            await transaccion.CommitAsync();
            return afectados > 0;
        }

        public async Task<bool> Reasignar(
    int idCaso,
    int cedulaEmpleadoActual,
    int cedulaNuevoEmpleado,
    int idEstadoAsignacion,
    DateTime fecha)
        {
            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            if (!await BloquearCaso(idCaso))
            {
                await transaccion.RollbackAsync();
                return false;
            }

            var responsable = await _context.Database.SqlQuery<int>($"""
                SELECT TOP 1 cedula_empleado AS [Value]
                FROM dbo.asignacioncaso WITH (UPDLOCK, HOLDLOCK)
                WHERE id_caso = {idCaso}
                ORDER BY id_asignacion DESC
                """).FirstOrDefaultAsync();

            if (responsable != cedulaEmpleadoActual)
            {
                await transaccion.RollbackAsync();
                return false;
            }

            // Buscar el estado Finalizada.
            int? idEstadoFinalizada = await _context.Estadoasignacions
                .Where(e => e.Nombre == "Finalizada")
                .Select(e => (int?)e.IdEstadoAsignacion)
                .FirstOrDefaultAsync();

            // Finalizar la asignación actual si existe ese estado.
            if (idEstadoFinalizada.HasValue)
            {
                await _context.Database.ExecuteSqlAsync($"""
                    UPDATE dbo.asignacioncaso
                    SET id_estado_asignacion = {idEstadoFinalizada.Value}
                    WHERE id_caso = {idCaso}
                      AND id_asignacion = (
                          SELECT MAX(id_asignacion)
                          FROM dbo.asignacioncaso
                          WHERE id_caso = {idCaso}
                      )
                    """);
            }

            int idAsignacion = await _context.Database.SqlQuery<int>($"""
                SELECT COALESCE(MAX(id_asignacion), 0) + 1 AS [Value]
                FROM dbo.asignacioncaso WITH (UPDLOCK, HOLDLOCK)
                """).SingleAsync();

            _context.Asignacioncasos.Add(new Asignacioncaso
            {
                IdAsignacion = idAsignacion,
                IdCaso = idCaso,
                CedulaEmpleado = cedulaNuevoEmpleado,
                FechaAsignacion = fecha,
                IdEstadoAsignacion = idEstadoAsignacion
            });

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return true;
        }

        public async Task<List<Estadocaso>> ListarEstadosCaso()
        {
            return await _context.Estadocasos
                .AsNoTracking()
                .OrderBy(e => e.IdEstadoCaso)
                .ToListAsync();
        }

        public async Task<List<Estadoasignacion>> ListarEstadosAsignacion()
        {
            return await _context.Estadoasignacions
                .AsNoTracking()
                .OrderBy(e => e.IdEstadoAsignacion)
                .ToListAsync();
        }

        public async Task<List<Empleado>> ListarEmpleadosActivos()
        {
            return await _context.Empleados
                .AsNoTracking()
                .Include(e => e.CedulaNavigation)
                .Include(e => e.IdEstadoActivoNavigation)
                .Include(e => e.IdRols)
                .Where(e => e.IdEstadoActivoNavigation.Nombre == "Activo")
                .OrderBy(e => e.CedulaNavigation.Nombre)
                .ThenBy(e => e.CedulaNavigation.PrimerApellido)
                .ThenBy(e => e.CedulaNavigation.SegundoApellido)
                .ToListAsync();
        }

        public async Task<Empleado?> ObtenerEmpleado(int cedula)
        {
            return await _context.Empleados
                .AsNoTracking()
                .Include(e => e.CedulaNavigation)
                .Include(e => e.IdEstadoActivoNavigation)
                .Include(e => e.IdRols)
                .FirstOrDefaultAsync(e => e.Cedula == cedula);
        }

        public async Task<Usuario?> ObtenerUsuario(int cedula)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.CedulaNavigation)
                .FirstOrDefaultAsync(u => u.Cedula == cedula);
        }

        public async Task<bool> ExisteUsuario(int cedula)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Cedula == cedula);
        }

        public async Task<Alertum?> ObtenerAlerta(int idAlerta)
        {
            return await _context.Alerta
                .AsNoTracking()
                .AsSplitQuery()
                .Include(a => a.IdEstadoAlertaNavigation)
                .Include(a => a.IdAnalisisNavigation)
                    .ThenInclude(r => r.IdNivelRiesgoNavigation)
                .Include(a => a.IdAnalisisNavigation)
                    .ThenInclude(r => r.IdMensajeNavigation)
                    .ThenInclude(m => m.IdConversacionNavigation)
                    .ThenInclude(c => c.CedulaUsuarioNavigation)
                    .ThenInclude(u => u.CedulaNavigation)
                .FirstOrDefaultAsync(a => a.IdAlerta == idAlerta);
        }

        public async Task<bool> AlertaTieneCaso(int idAlerta)
        {
            return await _context.Alertacasos
                .AsNoTracking()
                .AnyAsync(x => x.IdAlerta == idAlerta);
        }
    }
}
