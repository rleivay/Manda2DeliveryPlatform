using Manda2.Contracts.CheckOut;
using Manda2.Mobile.Services.Cart;
using Manda2.Mobile.Services.Common;
using Manda2.Mobile.Services.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Orders
{
    /// <summary>
    /// Implementación HTTP de IOrderGroupApiService.
    /// Registrar como Scoped en MauiProgram.cs.
    /// </summary>
    public class OrderGroupApiService : IOrderGroupApiService
    {
        private readonly HttpClient _http;
        private readonly ISessionService _session;

        public OrderGroupApiService(HttpClient http, ISessionService session)
        {
            _http = http;
            _session = session;
        }

        private async Task AttachTokenAsync()
        {
            var session = await _session.GetSessionAsync();
            if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", session.AccessToken);
            else
                _http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<CreateOrderGroupResult> CreateOrderGroupAsync(
            IReadOnlyList<CartStateMerchant> merchants,
            CancellationToken ct = default)
        {
            await AttachTokenAsync();

            var payload = new
            {
                DeliveryAddress = "Pendiente de selección",
                Lat = 0m,
                Lon = 0m,
                Merchants = merchants.Select(m => new
                {
                    MerchantId = m.MerchantId,
                    Items = m.Items.Select(i => new
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        SpecialInstructions = i.SpecialInstructions
                    }).ToList()
                }).ToList()
            };

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsJsonAsync(
                    "api/checkout/create-order-group", payload, ct);
            }
            catch (Exception ex)
            {
                return CreateOrderGroupResult.Failure($"Error de red: {ex.Message}");
            }

            var rawBody = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(rawBody))
                    return CreateOrderGroupResult.Failure("Respuesta vacía del servidor.");

                try
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<CreateOrderGroupResult>(
                        rawBody,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result == null)
                        return CreateOrderGroupResult.Failure("No se pudo deserializar la respuesta.");

                    return CreateOrderGroupResult.Success(
                        result.OrderGroupId,
                        result.TotalAmount,
                        result.ServiceFee,
                        result.DeliveryFee,
                        result.SubOrderCount);
                }
                catch (Exception ex)
                {
                    return CreateOrderGroupResult.Failure(
                        $"Error deserializando: {ex.Message}. Body: {rawBody[..Math.Min(200, rawBody.Length)]}");
                }
            }

            if (!string.IsNullOrWhiteSpace(rawBody))
            {
                try
                {
                    var errorResult = System.Text.Json.JsonSerializer.Deserialize<CreateOrderGroupResult>(
                        rawBody,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (errorResult != null) return errorResult;
                }
                catch { }
            }

            return CreateOrderGroupResult.Failure(
                $"Error HTTP {(int)response.StatusCode}: {rawBody[..Math.Min(300, rawBody.Length)]}");
        }
    }
}
