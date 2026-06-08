using Manda2.Contracts.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Mobile.Services.ShippingAddress.ShippingAddressApiService;

namespace Manda2.Mobile.Services.ShippingAddress
{
    public interface IShippingAddressApiService
    {
        Task<List<ShippingAddressDto>> GetMyShippingAddressesAsync();

        Task<int> CreateShippingAddressAsync(
            CreateShippingAddressRequest request,
            CancellationToken ct = default);
        /// <summary>
        /// Obtiene la configuración de Azure Maps desde el backend autenticado.
        /// La Subscription Key NUNCA se almacena en el cliente.
        /// </summary>
        Task<MapsConfigResponse?> GetMapsConfigAsync();

        /// <summary>
        /// Convierte coordenadas GPS a dirección textual.
        /// El backend llama a Azure Maps — la clave no sale al cliente.
        /// </summary>
        Task<ReverseGeocodeResponse?> GetReverseGeocodeAsync(double lat, double lon);

        // ── DTOs de respuesta ──────────────────────────────────────────────────────
        public sealed record MapsConfigResponse(string SubscriptionKey);
        public sealed record ReverseGeocodeResponse(string? Address, bool Resolved);
    }
}
