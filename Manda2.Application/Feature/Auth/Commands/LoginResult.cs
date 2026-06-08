using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        /// <summary>JWT de corta duración (60 min por defecto).</summary>
        public string? AccessToken { get; set; }

        /// <summary>Token opaco de larga duración para renovar el Access Token.</summary>
        public string? RefreshToken { get; set; }

        /// <summary>Fecha de expiración del Access Token (UTC).</summary>
        public DateTime? AccessTokenExpiresAt { get; set; }

        /// <summary>Rol del usuario autenticado (para routing en la app).</summary>
        public string? Role { get; set; }
    }
}
