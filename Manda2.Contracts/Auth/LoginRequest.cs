using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Auth
{
    /// <summary>
    /// Request para autenticación de usuario.
    /// Enviado al endpoint POST /api/auth/login
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
