// PROPÓSITO: Lee la Subscription Key de Azure Maps desde IConfiguration
//            (appsettings.json del backend) y la retorna al cliente.
//            El handler NO toca la base de datos — es configuración de
//            infraestructura, no de negocio.
//
// SEGURIDAD:
//   - El endpoint que llama a este handler debe requerir [Authorize].
//   - La clave nunca se almacena en BD ni en el cliente.

using Manda2.Application.Mediator;
using Microsoft.Extensions.Configuration;

namespace Manda2.Application.Feature.Maps.Queries
{
    /// <summary>
    /// Handler para <see cref="GetMapsConfigQuery"/>.
    /// Lee la clave de Azure Maps desde la configuración del servidor.
    /// </summary>
    public class GetMapsConfigQueryHandler
        : IQueryHandler<GetMapsConfigQuery, MapsConfigDto>
    {
        private readonly IConfiguration _configuration;

        public GetMapsConfigQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<MapsConfigDto> HandleAsync(
            GetMapsConfigQuery query,
            CancellationToken ct)
        {
            var key = _configuration["AzureMaps:SubscriptionKey"]
                      ?? string.Empty;

            return Task.FromResult(new MapsConfigDto(key));
        }
    }
}