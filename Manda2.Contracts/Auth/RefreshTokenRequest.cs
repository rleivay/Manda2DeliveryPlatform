using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Auth
{
    /// <summary>
    /// Request para renovar el AccessToken usando el RefreshToken.
    /// Enviado al endpoint POST /api/auth/refresh
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>Access Token expirado — el backend extrae el uid del claim.</summary>
        public string ExpiredAccessToken { get; set; } = string.Empty;

        /// <summary>Refresh Token activo del usuario.</summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
