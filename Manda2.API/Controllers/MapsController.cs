// PROPÓSITO: Entry point REST para servicios de mapas.
//
// ENDPOINTS:
//   GET  /api/maps/config           → Retorna Azure Maps Subscription Key (autenticado)
//   GET  /api/maps/reverse-geocode  → Convierte lat/lon a dirección textual (autenticado)
//
// SEGURIDAD:
//   - Ambos endpoints requieren token JWT válido (cualquier rol).
//   - La Subscription Key NUNCA se hardcodea en el cliente.
//
// PATRÓN: BaseApiController + ICommandBus (QueryAsync).

using Manda2.API.Services;
using Manda2.Application.Feature.Maps.Queries;
using Manda2.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// Servicios de mapas y geolocalización.
    /// Ruta base: /api/maps
    /// </summary>
    [Authorize]
    public class MapsController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public MapsController(ICommandBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// GET /api/maps/config
        /// Retorna la configuración de Azure Maps para el cliente autenticado.
        /// La Subscription Key viaja cifrada por HTTPS — nunca en el cliente.
        /// </summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetMapsConfig(CancellationToken ct)
        {
            var result = await _bus.QueryAsync<GetMapsConfigQuery, MapsConfigDto>(
                new GetMapsConfigQuery(), ct);

            return Ok(result);
        }

        /// <summary>
        /// GET /api/maps/reverse-geocode?lat={lat}&amp;lon={lon}
        /// Convierte coordenadas GPS a dirección textual usando Azure Maps REST API.
        /// El backend hace la llamada a Azure Maps — la clave nunca sale al cliente.
        /// </summary>
        [HttpGet("reverse-geocode")]
        public async Task<IActionResult> ReverseGeocode(
            [FromQuery] double lat,
            [FromQuery] double lon,
            CancellationToken ct)
        {
            var result = await _bus.QueryAsync<ReverseGeocodeQuery, ReverseGeocodeResult>(
                new ReverseGeocodeQuery(lat, lon), ct);

            return Ok(result);
        }
    }
}