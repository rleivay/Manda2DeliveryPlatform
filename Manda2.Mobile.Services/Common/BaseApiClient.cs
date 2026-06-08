using Manda2.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Common
{
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _httpClient;

        protected BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, request);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                return result ?? new ApiResponse<T> { Success = false, Message = "Respuesta vacía" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T> { Success = false, Message = $"Error de red: {ex.Message}" };
            }
        }
    }
}
