using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Proyecto.Autenticacion;
using Microsoft.EntityFrameworkCore;
using Proyecto.Models;
using FundacionPrevencion.BC.Configuracion;

var builder = WebApplication.CreateBuilder(args.Where(arg => arg != "--crear-empleado").ToArray());

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddChatbotServices();
builder.Services.AddAlertaServices();
builder.Services.AddScoped<Cuentas>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("acceso", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        {
            PermitLimit = 8,
            Window = TimeSpan.FromMinutes(1)
        }));
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CuentaUsuario", policy => policy
        .RequireClaim(Cuentas.Tipo, "usuario").RequireRole(Cuentas.RolUsuario));
    options.AddPolicy("CuentaPersonal", policy => policy
        .RequireClaim(Cuentas.Tipo, "empleado").RequireRole(Cuentas.Personal));
});
builder.Services.AddAuthentication("Cookies").AddCookie(options =>
{
    options.Cookie.Name = "fundacion_mvc";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.LoginPath = "/ingreso";
    options.AccessDeniedPath = "/acceso-denegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = async context =>
    {
        var cuentas = context.HttpContext.RequestServices.GetRequiredService<Cuentas>();
        if (context.Principal is null || !await cuentas.Vigente(context.Principal))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    };
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Chatbot"))
            context.Response.StatusCode = 401;
        else if (context.Request.Path.StartsWithSegments("/personal"))
            context.Response.Redirect("/personal/ingreso");
        else
            context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Chatbot"))
            context.Response.StatusCode = 403;
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

// Configuración de conexión a SQL Server Express.
builder.Services.AddDbContext<Proyecto.Models.ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConexionFundacion")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

if (args.Contains("--crear-empleado"))
{
    await AltaPersonal.Ejecutar(app.Services);
    return;
}

app.Run();
