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
    public class PaymentMethodApiService : IPaymentMethodApiService
    {
        private readonly HttpClient _http;
        private readonly ISessionService _session;

        public PaymentMethodApiService(HttpClient http, ISessionService session)
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

        public async Task<List<PaymentMethodDto>> GetActivePaymentMethodsAsync()
        {
            await AttachTokenAsync();
            var response = await _http.GetAsync("api/checkout/payment-methods");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<PaymentMethodDto>>()
                   ?? new List<PaymentMethodDto>();
        }
    }
}
