// PROPÓSITO: Implementación de IJwtService usando System.IdentityModel.Tokens.Jwt.
//
// CONFIGURACIÓN (appsettings.json → sección "Jwt"):
//   "Jwt": {
//     "Secret":          "tu-clave-secreta-minimo-32-chars",
//     "Issuer":          "Manda2API",
//     "Audience":        "Manda2Apps",
//     "AccessTokenMinutes":  60,
//     "RefreshTokenDays":    30
//   }
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Domain.Constants;
using Manda2.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Infrastructure.Security
{
    /// <summary>
    /// Implementación de IJwtService. Registrar como Scoped en DI.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        /// <inheritdoc/>
        public string GenerateAccessToken(AppUser user)
        {
            var secret = _config["Jwt:Secret"]!;
            var issuer = _config["Jwt:Issuer"]!;
            var audience = _config["Jwt:Audience"]!;
            var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ─── Claims base (siempre presentes) ─────────────────────────
            var claims = new List<Claim>
            {
                new(AppClaims.UserId,              user.Id.ToString()),
                new(ClaimTypes.Email,              user.Email),
                new(ClaimTypes.Role,               user.Role),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            // ─── Claims de actor (solo el que aplica) ────────────────────
            if (user.CustomerId.HasValue)
                claims.Add(new(AppClaims.CustomerId, user.CustomerId.Value.ToString()));

            if (user.DriverId.HasValue)
                claims.Add(new(AppClaims.DriverId, user.DriverId.Value.ToString()));

            if (user.MerchantId.HasValue)
                claims.Add(new(AppClaims.MerchantId, user.MerchantId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc/>
        public string GenerateRefreshToken()
        {
            // Token opaco de 64 bytes — criptográficamente seguro
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        /// <inheritdoc/>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var secret = _config["Jwt:Secret"]!;

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                // IMPORTANTE: no validar expiración en el flujo de refresh
                ValidateLifetime = false
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, validationParams, out var securityToken);

                // Verificar que el algoritmo sea el correcto
                if (securityToken is not JwtSecurityToken jwtToken
                    || !jwtToken.Header.Alg.Equals(
                           SecurityAlgorithms.HmacSha256,
                           StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
