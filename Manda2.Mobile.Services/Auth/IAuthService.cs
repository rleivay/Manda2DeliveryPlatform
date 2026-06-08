// PROPÓSITO: Contrato del servicio de autenticación HTTP.
//            Abstrae las llamadas a api/auth/* del backend.
//            Los componentes Razor inyectan esta interfaz, nunca
//            HttpClient directamente.
// ════════════════════════════════════════════════════════════════════

using Manda2.Contracts.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Auth
{
    /// <summary>
    /// Servicio de autenticación contra la API de Manda2.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>POST api/auth/login</summary>
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

        /// <summary>POST api/auth/register</summary>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

        /// <summary>
        /// POST api/auth/logout
        /// Requiere JWT Bearer en el header — lo agrega AuthService internamente
        /// leyendo el AccessToken desde ISessionService.
        /// </summary>
        Task LogoutAsync(CancellationToken ct = default);
    }
}
