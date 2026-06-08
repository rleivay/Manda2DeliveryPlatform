// PROPÓSITO: Renovar el Access Token usando el Refresh Token.
//            El cliente envía el Access Token expirado + Refresh Token válido.
//
// CONSUMIDOR: AuthController → POST api/auth/refresh
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    public class RefreshTokenCommand : ICommand<RefreshTokenResult>
    {
        /// <summary>Access Token expirado (para extraer uid del claim).</summary>
        public string ExpiredAccessToken { get; set; } = null!;

        /// <summary>Refresh Token activo del usuario.</summary>
        public string RefreshToken { get; set; } = null!;
    }
}
