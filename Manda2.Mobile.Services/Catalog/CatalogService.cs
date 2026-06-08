using Manda2.Contracts.Catalog;
using Manda2.Mobile.Services.Session;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Catalog
{
    /// <summary>
    /// Implementación HTTP de ICatalogService.
    /// Registrar como Scoped en MauiProgram.cs.
    /// </summary>
    /// <summary>
    /// Implementación HTTP de ICatalogService.
    /// Registrar como Scoped en MauiProgram.cs.
    /// </summary>
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _http;
        private readonly ISessionService _session;
        private readonly string _imageBaseUrl;

        public CatalogService(HttpClient http, ISessionService session, IConfiguration config)
        {
            _http = http;
            _session = session;
            // Toma la URL base de imágenes desde appsettings.json → ApiSettings:ImagesBaseUrl
            // Fallback: BaseAddress del HttpClient (mismo host del API)
            _imageBaseUrl = config["ApiSettings:ImagesBaseUrl"]
                            ?? _http.BaseAddress?.ToString()
                            ?? string.Empty;
        }

        // ─── Helpers privados ────────────────────────────────

        /// <summary>
        /// Convierte una ruta relativa de imagen en URL absoluta HTTPS.
        /// Si ya es absoluta la retorna sin cambios.
        /// Si es null/vacía retorna null (la UI usará su propio fallback).
        /// </summary>
        private string? FormatImageUrl(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return null;

            if (relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return relativeUrl;

            var baseUrl = _imageBaseUrl.TrimEnd('/');
            var path = relativeUrl.TrimStart('/');
            return $"{baseUrl}/{path}";
        }

        /// <summary>
        /// Agrega el JWT Bearer al header Authorization antes de cada llamada.
        /// Si no hay sesión activa, limpia el header (llamada anónima fallará en el backend).
        /// </summary>
        private async Task AttachTokenAsync()
        {
            var session = await _session.GetSessionAsync();
            if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", session.AccessToken);
            else
                _http.DefaultRequestHeaders.Authorization = null;
        }

        // ─── Implementación de ICatalogService ───────────────

        /// <inheritdoc/>
        public async Task<List<MerchantSummaryDto>> GetOpenMerchantsAsync()
        {
            try
            {
                await AttachTokenAsync();
                var result = await _http.GetFromJsonAsync<List<MerchantSummaryDto>>(
                    "api/v1/catalog/merchants/open");

                if (result != null)
                    foreach (var m in result)
                        m.LogoUrl = FormatImageUrl(m.LogoUrl);

                return result ?? new List<MerchantSummaryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] GetOpenMerchantsAsync error: {ex.Message}");
                return new List<MerchantSummaryDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<MerchantSummaryDto>> GetMerchantsByCategoryAsync(int? categoryId = null)
        {
            try
            {
                await AttachTokenAsync();

                // Construir URL con query param opcional
                var url = categoryId.HasValue
                    ? $"api/v1/catalog/merchants?categoryId={categoryId.Value}"
                    : "api/v1/catalog/merchants";

                var result = await _http.GetFromJsonAsync<List<MerchantSummaryDto>>(url);
                return result ?? new List<MerchantSummaryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] GetMerchantsByCategoryAsync error: {ex.Message}");
                return new List<MerchantSummaryDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<MerchantProductDto>> GetMerchantProductsAsync(int merchantId)
        {
            try
            {
                await AttachTokenAsync();
                var result = await _http.GetFromJsonAsync<List<MerchantProductDto>>(
                    $"api/v1/catalog/products/{merchantId}");
                return result ?? new List<MerchantProductDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] GetMerchantProductsAsync error: {ex.Message}");
                return new List<MerchantProductDto>();
            }
        }

        public async Task<List<MerchantCategoryDto>> GetMerchantCategoriesAsync()
        {
            try
            {
                await AttachTokenAsync();
                var result = await _http.GetFromJsonAsync<List<MerchantCategoryDto>>(
                    "api/v1/catalog/merchant-categories");
                return result ?? new List<MerchantCategoryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] GetMerchantCategoriesAsync error: {ex.Message}");
                return new List<MerchantCategoryDto>();
            }
        }

        public async Task<int> GetSystemConfigIntAsync(string key, int defaultValue)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<ConfigResponse>($"api/v1/config/{key}");
                if (response != null && int.TryParse(response.Value, out var val))
                    return val;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] Error leyendo config {key}: {ex.Message}");
            }
            return defaultValue;
        }

        private record ConfigResponse(string Key, string Value);
    }
}
