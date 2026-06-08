using Manda2.Contracts.Customer;
using Manda2.Mobile.Services.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Mobile.Services.ShippingAddress.IShippingAddressApiService;

namespace Manda2.Mobile.Services.ShippingAddress
{
    public class ShippingAddressApiService : IShippingAddressApiService
    {
        private readonly HttpClient _http;
        private readonly ISessionService _session;

        public ShippingAddressApiService(HttpClient http, ISessionService session)
        {
            _http = http;
            _session = session;
        }

        private async Task AttachTokenAsync()
        {
            var session = await _session.GetSessionAsync();
            _http.DefaultRequestHeaders.Authorization = session != null
                ? new AuthenticationHeaderValue("Bearer", session.AccessToken)
                : null;
        }

        public async Task<List<ShippingAddressDto>> GetMyShippingAddressesAsync()
        {
            await AttachTokenAsync();

            var response = await _http.GetAsync("api/customers/me/shipping-addresses");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ShippingAddressDto>>()
                   ?? new List<ShippingAddressDto>();
        }

        public async Task<int> CreateShippingAddressAsync(
            CreateShippingAddressRequest request,
            CancellationToken ct = default)
        {
            await AttachTokenAsync();

            var response = await _http.PostAsJsonAsync(
                "api/customers/me/shipping-addresses",
                request,
                ct);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
        }


        public async Task<MapsConfigResponse?> GetMapsConfigAsync()
        {
            await AttachTokenAsync();
            return await _http.GetFromJsonAsync<MapsConfigResponse>("api/maps/config");
        }

        public async Task<ReverseGeocodeResponse?> GetReverseGeocodeAsync(double lat, double lon)
        {
            await AttachTokenAsync();
            var url = $"api/maps/reverse-geocode?lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&lon={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            return await _http.GetFromJsonAsync<ReverseGeocodeResponse>(url);
        }



    }
}
