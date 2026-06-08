using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    /// <summary>
    /// Extensión que registra automáticamente todos los Handlers
    /// del Assembly de Application en el contenedor DI de .NET.
    /// Reemplaza Scrutor. Sin dependencias externas.
    /// </summary>
    public static class MediatorExtensions
    {
        public static IServiceCollection AddManda2Mediator(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .ToList();

                foreach (var type in types)
                {
                    // Registra ICommandHandler<TCommand, TResult>
                    RegisterHandlers(services, type,
                        typeof(ICommandHandler<,>));

                    // Registra ICommandHandler<TCommand>
                    RegisterHandlers(services, type,
                        typeof(ICommandHandler<>));

                    // Registra IQueryHandler<TQuery, TResult>
                    RegisterHandlers(services, type,
                        typeof(IQueryHandler<,>));
                }
            }

            // Registra el Bus como punto único de entrada
            services.AddScoped<ICommandBus, CommandBus>();

            return services;
        }

        private static void RegisterHandlers(
            IServiceCollection services,
            Type implementation,
            Type openGenericInterface)
        {
            var interfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == openGenericInterface);

            foreach (var iface in interfaces)
            {
                services.AddScoped(iface, implementation);
            }
        }
    }
}
