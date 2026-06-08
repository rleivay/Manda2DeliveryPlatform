// PROPÓSITO: Implementación de IAuthService.
//            Realiza las llamadas HTTP a api/auth/*.
//            Inyecta ISessionService para leer el AccessToken
//            en el header Authorization del Logout.

using Manda2.Contracts.Auth;
using Manda2.Mobile.Services.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Auth
{
    /// <summary>
    /// Implementación HTTP de IAuthService.
    /// Registrar como Scoped en MauiProgram.cs.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ISessionService _session;

        public AuthService(HttpClient http, ISessionService session)
        {
            _http = http;
            _session = session;
        }

        /// <inheritdoc/>
        public async Task<LoginResponse> LoginAsync(
            LoginRequest request, CancellationToken ct = default)
        {
            // POST api/auth/login — sin token (AllowAnonymous)
            var response = await _http.PostAsJsonAsync("api/auth/login", request, ct);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
                return result ?? new LoginResponse { Success = false, Message = "Respuesta vacía." };
            }

            // Leer mensaje de error del backend
            var error = await response.Content
                .ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            return error ?? new LoginResponse
            {
                Success = false,
                Message = $"Error HTTP {(int)response.StatusCode}"
            };
        }

        /// <inheritdoc/>
        public async Task<RegisterResponse> RegisterAsync(
            RegisterRequest request, CancellationToken ct = default)
        {
            // POST api/auth/register — sin token (AllowAnonymous)
            var response = await _http.PostAsJsonAsync("api/auth/register", request, ct);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<RegisterResponse>(cancellationToken: ct);
                return result ?? new RegisterResponse { Success = true };
            }

            var error = await response.Content
                .ReadFromJsonAsync<RegisterResponse>(cancellationToken: ct);
            return error ?? new RegisterResponse
            {
                Success = false,
                Message = $"Error HTTP {(int)response.StatusCode}"
            };
        }

        /// <inheritdoc/>
        public async Task LogoutAsync(CancellationToken ct = default)
        {
            // POST api/auth/logout — requiere JWT Bearer
            var sessionData = await _session.GetSessionAsync();
            if (sessionData == null) return;

            // Agregar token al header de esta llamada específica
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", sessionData.AccessToken);

            await _http.PostAsync("api/auth/logout", null, ct);

            // Limpiar header después del logout
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}