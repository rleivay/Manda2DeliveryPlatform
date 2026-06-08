using Manda2.Contracts.Auth;
using Manda2.Mobile.Services.Cart;
using Manda2.Mobile.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Session
{
    public class SessionService : ISessionService
    {
        // ── Propiedad en memoria ─────────────────────────────────────────
        /// <inheritdoc/>
        public SessionConfig? Config { get; private set; }

        private const string SessionKey = "manda2_session";
        
        private readonly IServiceScopeFactory _scopeFactory;  // 🆕
        private readonly ICartService _cartService;  // 🆕 inyección directa

        public SessionService(IServiceScopeFactory scopeFactory, ICartService cartService)
        {
            _scopeFactory = scopeFactory;
            _cartService = cartService;
        }

        // ── Implementación StartSessionAsync ────────────────────────────
        /// <inheritdoc/>
        public async Task StartSessionAsync(CancellationToken ct = default)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var catalogService = scope.ServiceProvider
                    .GetRequiredService<ICatalogService>();

                var merchantLimit = await catalogService
                    .GetSystemConfigIntAsync("DEFAULT_DRIVER_MAX_SUBORDER_LIMIT", 1);

                Config = new SessionConfig { MerchantLimit = merchantLimit };

                // Ahora sí es la misma instancia ✅
                await _cartService.InitializeAsync(merchantLimit);

                Console.WriteLine($"[SessionService] MerchantLimit: {merchantLimit}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionService] Error: {ex.Message}");
                Config = new SessionConfig { MerchantLimit = 1 };
                await _cartService.InitializeAsync(1);
            }
        }

        /// <inheritdoc/>
        public event Action<string?> OnSessionChanged = delegate { };

        /// <inheritdoc/>
        public async Task SaveSessionAsync(
            LoginResponse loginResponse, CancellationToken ct = default)
        {
            var session = new SessionData
            {
                Role = loginResponse.Role ?? string.Empty,
                AccessToken = loginResponse.AccessToken ?? string.Empty,
                RefreshToken = loginResponse.RefreshToken ?? string.Empty,
                AccessTokenExpiresAt = loginResponse.AccessTokenExpiresAt
            };

            var json = JsonSerializer.Serialize(session);
            await SecureStorage.Default.SetAsync(SessionKey, json);

            // 🔔 Notificar a los suscriptores con el nuevo rol
            OnSessionChanged.Invoke(session.Role);
        }

        /// <inheritdoc/>
        public async Task<SessionData?> GetSessionAsync(CancellationToken ct = default)
        {
            try
            {
                var json = await SecureStorage.Default.GetAsync(SessionKey);
                if (string.IsNullOrEmpty(json)) return null;
                return JsonSerializer.Deserialize<SessionData>(json);
            }
            catch { return null; }
        }

        /// <inheritdoc/>
        public async Task ClearSessionAsync(CancellationToken ct = default)
        {
            SecureStorage.Default.Remove(SessionKey);
            await Task.CompletedTask;

            // 🔔 Notificar a los suscriptores: sesión cerrada (null)
            OnSessionChanged.Invoke(null);
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default)
        {
            var session = await GetSessionAsync(ct);
            if (session == null) return false;
            if (string.IsNullOrEmpty(session.AccessToken)) return false;
            if (session.AccessTokenExpiresAt.HasValue)
                return session.AccessTokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(1);
            return true;
        }

        public int? SelectedShippingAddressId { get; set; }
        public string? SelectedAddressText { get; set; }
    }
}
