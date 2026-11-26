using FundacionPrevencion.DA;
using Microsoft.Extensions.DependencyInjection;

namespace FundacionPrevencion.BC.Configuracion
{
    public static class AlertaServiceExtension
    {
        public static IServiceCollection AddAlertaServices(
            this IServiceCollection services)
        {
            services.AddScoped<AlertaDA>();
            services.AddScoped<AlertaBC>();

            return services;
        }
    }
}
