// PROPÓSITO: Comando de autenticación. Recibe email + password,
//            retorna Access Token + Refresh Token.
//
// CONSUMIDOR: AuthController → POST api/auth/login
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    /// <summary>
    /// Comando de login con usuario y contraseña.
    /// </summary>
    public class LoginCommand : ICommand<LoginResult>
    {
        /// <summary>Email del usuario (usado como username).</summary>
        public string Email { get; set; } = null!;

        /// <summary>Contraseña en texto plano (se compara contra BCrypt hash).</summary>
        public string Password { get; set; } = null!;
    }
}
