using Manda2.Contracts.CheckOut;
using Manda2.Mobile.Services.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.CheckOut
{
    public class CheckoutApiService : ICheckoutApiService
    {
        private readonly HttpClient _http;
        private readonly ISessionService _session;

        public CheckoutApiService(HttpClient http, ISessionService session)
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

        public async Task<ConfirmOrderGroupResult> ConfirmOrderGroupLogisticsAsync(ConfirmOrderGroupRequest request)
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/checkout/confirm-order-group", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ConfirmOrderGroupResult>();
        }

        public async Task<ConfirmCheckoutResult> FinalizeCheckoutAsync(ConfirmCheckoutRequest request)
        {
            await AttachTokenAsync();
            var response = await _http.PostAsJsonAsync("api/checkout", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ConfirmCheckoutResult>();
        }
    }
}
