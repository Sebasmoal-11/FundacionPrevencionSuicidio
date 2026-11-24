
using Microsoft.Extensions.DependencyInjection;
using FundacionPrevencion.BW;
using FundacionPrevencion.DA;
using FundacionPrevencion.BW.Servicios;

namespace FundacionPrevencion.BC.Configuracion
{
    public static class ChatbotServiceExtension
    {
        public static IServiceCollection AddChatbotServices(
            this IServiceCollection services)
        {
            services.AddScoped<ChatbotDA>();
            services.AddScoped<ChatbotBW>();
            services.AddScoped<ChatbotBC>();

            services.AddHttpClient<GeminiService>();

            services.AddScoped<AnalisisRiesgoService>();

            return services;
        }
    }
}
