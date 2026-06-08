// PROPÓSITO: Convierte coordenadas GPS (lat, lon) a dirección textual
//            llamando a la Azure Maps REST API desde el backend.
//            El cliente NUNCA llama a Azure Maps directamente.
//
// CONSUMIDOR: MapsController → GET /api/maps/reverse-geocode
// PATRÓN: IQuery<TResult> / IQueryHandler — Mediador propio Manda2.

using Manda2.Application.Mediator;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Manda2.Application.Feature.Maps.Queries
{
    // ── Query ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parámetros para convertir coordenadas a dirección textual.
    /// </summary>
    public record ReverseGeocodeQuery(double Latitude, double Longitude)
        : IQuery<ReverseGeocodeResult>;

    /// <summary>
    /// Resultado del reverse geocoding.
    /// </summary>
    public record ReverseGeocodeResult(string? Address, bool Resolved);

    // ── Handler ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Llama a Azure Maps Search Address Reverse API.
    /// Endpoint: GET https://atlas.microsoft.com/search/address/reverse/json
    /// Documentación: https://learn.microsoft.com/azure/azure-maps/how-to-search-for-address
    /// </summary>
    public class ReverseGeocodeQueryHandler
        : IQueryHandler<ReverseGeocodeQuery, ReverseGeocodeResult>
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReverseGeocodeQueryHandler(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ReverseGeocodeResult> HandleAsync(
            ReverseGeocodeQuery query,
            CancellationToken ct)
        {
            var key = _configuration["AzureMaps:SubscriptionKey"];

            if (string.IsNullOrWhiteSpace(key))
                return new ReverseGeocodeResult(null, false);

            try
            {
                var client = _httpClientFactory.CreateClient("AzureMaps");

                // Azure Maps espera: query=lat,lon (no lon,lat)
                var url = $"https://atlas.microsoft.com/search/address/reverse/json" +
                          $"?api-version=1.0" +
                          $"&subscription-key={key}" +
                          $"&query={query.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                          $",{query.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                          $"&language=es-419";

                var response = await client.GetFromJsonAsync<AzureMapsReverseResponse>(url, ct);

                var address = response?.Addresses?.FirstOrDefault()?.Address?.FreeformAddress;

                return string.IsNullOrWhiteSpace(address)
                    ? new ReverseGeocodeResult(null, false)
                    : new ReverseGeocodeResult(address, true);
            }
            catch
            {
                return new ReverseGeocodeResult(null, false);
            }
        }

        // ── DTOs internos de deserialización Azure Maps ───────────────────────

        private sealed class AzureMapsReverseResponse
        {
            public List<AzureMapsAddressItem>? Addresses { get; set; }
        }

        private sealed class AzureMapsAddressItem
        {
            public AzureMapsAddress? Address { get; set; }
        }

        private sealed class AzureMapsAddress
        {
            public string? FreeformAddress { get; set; }
        }
    }
}