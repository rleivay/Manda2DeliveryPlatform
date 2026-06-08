// PROPÓSITO: Lógica de autenticación.
//
// FLUJO:
//   1. Buscar AppUser por email.
//   2. Verificar cuenta activa y no bloqueada.
//   3. Verificar contraseña con BCrypt.
//   4. Si falla: incrementar FailedLoginAttempts; bloquear si supera límite.
//   5. Si ok: resetear contador, generar tokens, persistir RefreshToken.
//   6. Retornar tokens.
//
// SEGURIDAD:
//   - Mensaje de error genérico (no revelar si el email existe).
//   - Bloqueo temporal tras 5 intentos fallidos (configurable en AppConfig).
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    /// <summary>
    /// Handler del comando LoginCommand.
    /// </summary>
    public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly IJwtService _jwt;
        private readonly IConfiguration _config;

        public LoginCommandHandler(
            IApplicationDbContext db,
            IJwtService jwt,
            IConfiguration config)
        {
            _db = db;
            _jwt = jwt;
            _config = config;
        }

        public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Buscar usuario por email (case-insensitive).
            // ─────────────────────────────────────────────────────────────
            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == command.Email.ToLower(), ct);

            // Mensaje genérico — no revelar si el email existe
            if (user == null)
                return Fail("Credenciales inválidas.");

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Verificar cuenta activa.
            // ─────────────────────────────────────────────────────────────
            if (!user.IsActive)
                return Fail("Cuenta suspendida. Contacta al administrador.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Verificar bloqueo temporal.
            // ─────────────────────────────────────────────────────────────
            if (user.IsLocked && user.LockedUntil.HasValue)
            {
                if (DateTime.UtcNow < user.LockedUntil.Value)
                    return Fail($"Cuenta bloqueada temporalmente. Intenta después de {user.LockedUntil.Value:HH:mm} UTC.");

                // Desbloquear automáticamente si ya expiró el bloqueo
                user.IsLocked = false;
                user.FailedLoginAttempts = 0;
                user.LockedUntil = null;
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Verificar contraseña con BCrypt.
            // ─────────────────────────────────────────────────────────────
            bool passwordValid = BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash);

            if (!passwordValid)
            {
                // Incrementar contador de intentos fallidos
                user.FailedLoginAttempts++;

                // Límite configurable (default: 5)
                int maxAttempts = int.Parse(_config["Security:MaxFailedLoginAttempts"] ?? "5");
                int lockMinutes = int.Parse(_config["Security:LockoutMinutes"] ?? "15");

                if (user.FailedLoginAttempts >= maxAttempts)
                {
                    user.IsLocked = true;
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(lockMinutes);
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);

                return Fail("Credenciales inválidas.");
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Login exitoso — resetear contador y generar tokens.
            // ─────────────────────────────────────────────────────────────
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            int refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30");
            int accessMins = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");

            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            user.LockedUntil = null;
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshDays);
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return new LoginResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(accessMins),
                Role = user.Role
            };
        }

        private static LoginResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
