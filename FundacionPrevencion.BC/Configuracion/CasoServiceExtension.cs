using FundacionPrevencion.DA;
using Microsoft.Extensions.DependencyInjection;

namespace FundacionPrevencion.BC.Configuracion
{
    public static class CasoServiceExtension
    {
        public static IServiceCollection AddCasoServices(
            this IServiceCollection services)
        {
            services.AddScoped<CasoDA>();
            services.AddScoped<CasoBC>();

            return services;
        }
    }
}
