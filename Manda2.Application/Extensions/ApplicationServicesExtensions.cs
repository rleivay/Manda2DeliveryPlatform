using Manda2.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<AvailabilityService>();
            //services.AddScoped<CommissionResolverService>();
            // Aquí irán todos los servicios de dominio futuros
            return services;
        }
    }
}
