// FLUJO:
//   1. Extraer uid del Access Token expirado (sin validar expiración).
//   2. Buscar AppUser por uid.
//   3. Comparar RefreshToken + verificar que no expiró.
//   4. Generar nuevo par de tokens (rotación de refresh token).
//   5. Persistir nuevo RefreshToken.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    public class RefreshTokenCommandHandler
        : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly IJwtService _jwt;
        private readonly IConfiguration _config;

        public RefreshTokenCommandHandler(
            IApplicationDbContext db,
            IJwtService jwt,
            IConfiguration config)
        {
            _db = db;
            _jwt = jwt;
            _config = config;
        }

        public async Task<RefreshTokenResult> HandleAsync(
            RefreshTokenCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Extraer claims del token expirado.
            // ─────────────────────────────────────────────────────────────
            var principal = _jwt.GetPrincipalFromExpiredToken(command.ExpiredAccessToken);
            if (principal == null)
                return Fail("Token inválido.");

            var uidClaim = principal.FindFirst(AppClaims.UserId)?.Value;
            if (!int.TryParse(uidClaim, out int userId))
                return Fail("Token inválido.");

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Buscar usuario.
            // ─────────────────────────────────────────────────────────────
            var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null || !user.IsActive)
                return Fail("Usuario no encontrado o inactivo.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar Refresh Token.
            // ─────────────────────────────────────────────────────────────
            if (user.RefreshToken != command.RefreshToken)
                return Fail("Refresh token inválido.");

            if (!user.RefreshTokenExpiresAt.HasValue
                || user.RefreshTokenExpiresAt.Value < DateTime.UtcNow)
                return Fail("Refresh token expirado. Inicia sesión nuevamente.");

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Generar nuevo par de tokens (rotación).
            // ─────────────────────────────────────────────────────────────
            var newAccessToken = _jwt.GenerateAccessToken(user);
            var newRefreshToken = _jwt.GenerateRefreshToken();

            int refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30");
            int accessMins = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Persistir nuevo RefreshToken (invalida el anterior).
            // ─────────────────────────────────────────────────────────────
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new RefreshTokenResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessMins)
            };
        }

        private static RefreshTokenResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
