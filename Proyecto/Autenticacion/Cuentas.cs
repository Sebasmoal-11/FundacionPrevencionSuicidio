using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using Proyecto.Models;

namespace Proyecto.Autenticacion;

public sealed class Cuentas(ApplicationDbContext db)
{
    public const string RolUsuario = "Usuario";
    public const string Personal = "Personal";
    public const string Tipo = "tipo_cuenta";
    public const string Sesion = "sesion_id";
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Referencia = new();
    private static readonly string HashFicticio = Hasher.HashPassword(Referencia, Guid.NewGuid().ToString());

    public async Task<string?> Crear(RegistroForm datos, int cedula, bool empleado = false)
    {
        var hash = Hasher.HashPassword(Referencia, datos.Contrasena);
        var correo = datos.Correo.Trim().ToLowerInvariant();
        var nombre = datos.Nombre.Trim();
        var primero = datos.PrimerApellido.Trim();
        var segundo = datos.SegundoApellido.Trim();
        if (nombre.Length == 0 || primero.Length == 0 || segundo.Length == 0)
            return "Ingrese los nombres y apellidos.";

        await using var transaccion = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (await db.Usuarios.FromSql($"""
                SELECT * FROM dbo.usuario WITH (UPDLOCK, HOLDLOCK) WHERE LOWER(TRIM(correo)) = {correo}
                """).AnyAsync()
            || await db.Empleados.FromSql($"""
                SELECT * FROM dbo.empleado WITH (UPDLOCK, HOLDLOCK) WHERE LOWER(TRIM(correo)) = {correo}
                """).AnyAsync())
            return "Ya existe una cuenta con ese correo.";

        var persona = await db.Personas.FromSql($"""
            SELECT * FROM dbo.persona WITH (UPDLOCK, HOLDLOCK) WHERE cedula = {cedula}
            """).SingleOrDefaultAsync();
        if (persona is null)
            db.Personas.Add(new Persona { Cedula = cedula, Nombre = nombre, PrimerApellido = primero, SegundoApellido = segundo });
        else if (!string.Equals(persona.Nombre, nombre, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(persona.PrimerApellido, primero, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(persona.SegundoApellido, segundo, StringComparison.OrdinalIgnoreCase))
            return "Revise los datos de identificación ingresados.";

        int activo = await db.Estadoactivos.Where(e => e.Nombre == "Activo").Select(e => e.IdEstadoActivo).SingleAsync();
        if (empleado)
        {
            var rol = await db.Rols.FromSql($"""
                SELECT * FROM dbo.rol WITH (UPDLOCK, HOLDLOCK) WHERE nombre = {Personal}
                """).SingleOrDefaultAsync();
            if (rol is null)
            {
                int id = (await db.Rols.MaxAsync(r => (int?)r.IdRol) ?? 0) + 1;
                rol = new Rol { IdRol = id, Nombre = Personal, Descripcion = "Acceso al área del personal" };
                db.Rols.Add(rol);
            }
            db.Empleados.Add(new Empleado
            {
                Cedula = cedula, Correo = correo, ContrasenaHash = hash,
                FechaContratacion = DateTime.UtcNow, IdEstadoActivo = activo, IdRols = [rol]
            });
        }
        else
        {
            db.Usuarios.Add(new Usuario
            {
                Cedula = cedula, Correo = correo, ContrasenaHash = hash,
                FechaRegistro = DateTime.UtcNow, IdEstadoActivo = activo
            });
        }

        try
        {
            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return null;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return "Ya existe una cuenta con esa cédula o correo.";
        }
    }

    public async Task<(ClaimsPrincipal? Cuenta, string? Error)> Iniciar(IngresoForm datos, bool empleado, DateTimeOffset vence)
    {
        string correo = datos.Correo.Trim().ToLowerInvariant();
        var usuario = empleado ? null : await db.Usuarios.Include(u => u.CedulaNavigation)
            .Include(u => u.IdEstadoActivoNavigation).SingleOrDefaultAsync(u => u.Correo == correo);
        var personal = !empleado ? null : await db.Empleados.Include(e => e.CedulaNavigation).Include(e => e.IdRols)
            .Include(e => e.IdEstadoActivoNavigation).SingleOrDefaultAsync(e => e.Correo == correo);
        var cuenta = usuario?.Cedula ?? personal?.Cedula;
        var resultado = Verificar(usuario?.ContrasenaHash ?? personal?.ContrasenaHash ?? HashFicticio, datos.Contrasena);
        const string credencialesIncorrectas = "Correo o contraseña incorrectos.";
        if (cuenta is null || resultado == PasswordVerificationResult.Failed)
            return (null, credencialesIncorrectas);

        var estado = usuario?.IdEstadoActivoNavigation.Nombre ?? personal?.IdEstadoActivoNavigation.Nombre;
        if (estado != "Activo")
            return (null, "La cuenta no está habilitada");

        if (personal is not null && !personal.IdRols.Any(r => r.Nombre == Personal))
            return (null, credencialesIncorrectas);

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var nuevo = Hasher.HashPassword(Referencia, datos.Contrasena);
            if (usuario is not null) usuario.ContrasenaHash = nuevo;
            if (personal is not null) personal.ContrasenaHash = nuevo;
            await db.SaveChangesAsync();
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hash = HashToken(token);
        int? usuarioId = usuario?.Cedula;
        int? empleadoId = personal?.Cedula;
        await db.Database.ExecuteSqlAsync($"""
            DELETE FROM dbo.sesionautenticacion WHERE vence <= SYSUTCDATETIME();
            INSERT dbo.sesionautenticacion (token_hash, cedula_usuario, cedula_empleado, vence)
            VALUES ({hash}, {usuarioId}, {empleadoId}, {vence.UtcDateTime});
            """);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, cuenta.ToString()!),
            new(ClaimTypes.Name, usuario?.CedulaNavigation.Nombre ?? personal!.CedulaNavigation.Nombre),
            new(Tipo, empleado ? "empleado" : "usuario"),
            new(Sesion, token)
        };
        if (empleado)
            claims.AddRange(personal!.IdRols.Select(rol => new Claim(ClaimTypes.Role, rol.Nombre)));
        else
            claims.Add(new Claim(ClaimTypes.Role, RolUsuario));
        return (new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), null);
    }

    public async Task<bool> Vigente(ClaimsPrincipal cuenta)
    {
        var token = cuenta.FindFirstValue(Sesion);
        if (token is null || !int.TryParse(cuenta.FindFirstValue(ClaimTypes.NameIdentifier), out int cedula))
            return false;
        var hash = HashToken(token);
        if (cuenta.FindFirstValue(Tipo) == "empleado")
        {
            return await db.Database.SqlQuery<int>($"""
                SELECT 1 AS [Value] FROM dbo.sesionautenticacion s
                JOIN dbo.empleado e ON e.cedula = s.cedula_empleado
                JOIN dbo.estadoactivo a ON a.id_estado_activo = e.id_estado_activo
                WHERE s.token_hash = {hash} AND s.vence > SYSUTCDATETIME()
                  AND e.cedula = {cedula} AND a.nombre = N'Activo'
                  AND EXISTS (SELECT 1 FROM dbo.empleadorol er JOIN dbo.rol r ON r.id_rol = er.id_rol
                              WHERE er.cedula_empleado = e.cedula AND r.nombre = N'Personal')
                """).AnyAsync();
        }

        return await db.Database.SqlQuery<int>($"""
            SELECT 1 AS [Value] FROM dbo.sesionautenticacion s
            JOIN dbo.usuario u ON u.cedula = s.cedula_usuario
            JOIN dbo.estadoactivo a ON a.id_estado_activo = u.id_estado_activo
            WHERE s.token_hash = {hash} AND s.vence > SYSUTCDATETIME()
              AND u.cedula = {cedula} AND a.nombre = N'Activo'
            """).AnyAsync();
    }

    public async Task Cerrar(ClaimsPrincipal cuenta)
    {
        if (cuenta.FindFirstValue(Sesion) is { } token)
            await db.Database.ExecuteSqlAsync($"DELETE FROM dbo.sesionautenticacion WHERE token_hash = {HashToken(token)}");
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static PasswordVerificationResult Verificar(string hash, string contrasena)
    {
        const string formatoAnterior = "scrypt:32768:8:1$";
        try
        {
            if (!hash.StartsWith(formatoAnterior, StringComparison.Ordinal))
                return Hasher.VerifyHashedPassword(Referencia, hash, contrasena);

            string[] partes = hash.Split('$');
            if (partes.Length != 3 || partes[2].Length != 128)
                return PasswordVerificationResult.Failed;
            byte[] calculado = SCrypt.Generate(
                Encoding.UTF8.GetBytes(contrasena), Encoding.UTF8.GetBytes(partes[1]), 32768, 8, 1, 64);
            return CryptographicOperations.FixedTimeEquals(calculado, Convert.FromHexString(partes[2]))
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
    }
}
