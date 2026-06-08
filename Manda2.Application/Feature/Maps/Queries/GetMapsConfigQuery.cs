// PROPÓSITO: Query de solo lectura que retorna la configuración pública
//            del servicio de mapas (Azure Maps Subscription Key).
//            La clave NUNCA se expone en el cliente — siempre viaja
//            desde el backend autenticado.
//
// CONSUMIDOR: MapsController → GET /api/maps/config
// PATRÓN: IQuery<TResult> / IQueryHandler — Mediador propio Manda2.

using Manda2.Application.Mediator;

namespace Manda2.Application.Feature.Maps.Queries
{
    /// <summary>
    /// Retorna la configuración del servicio de mapas para el cliente autenticado.
    /// </summary>
    public record GetMapsConfigQuery() : IQuery<MapsConfigDto>;

    /// <summary>
    /// DTO de respuesta con la configuración pública de Azure Maps.
    /// Solo expone lo estrictamente necesario para el frontend.
    /// </summary>
    public record MapsConfigDto(string SubscriptionKey);
}