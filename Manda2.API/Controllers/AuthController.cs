// PROPÓSITO: Entry point REST para autenticación.
//
// ENDPOINTS:
//   POST api/auth/register → RegisterCommand
//   POST api/auth/login    → LoginCommand
//   POST api/auth/refresh  → RefreshTokenCommand
//   POST api/auth/logout   → Invalida RefreshToken (inline, sin command)
//
// SEGURIDAD: Todos los endpoints son [AllowAnonymous].
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.API.Extensions;
using Manda2.Application.Common;
using Manda2.Application.Feature.Auth.Commands;
using Manda2.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// Controlador de autenticación. Todos los endpoints son públicos
    /// excepto Logout que requiere token válido.
    /// </summary>
    
    public class AuthController : BaseApiController
    {
        private readonly ICommandBus _bus;
        private readonly IApplicationDbContext _db;

        public AuthController(ICommandBus bus, IApplicationDbContext db)
        {
            _bus = bus;
            _db = db;
        }

        /// <summary>POST api/auth/register</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand command, CancellationToken ct)
        {
            var result = await _bus.SendAsync<RegisterCommand, RegisterResult>(command, ct);
            return result.Success ? Ok(result) : BadRequest(new { result.Message });
        }

        /// <summary>POST api/auth/login</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command, CancellationToken ct)
        {
            var result = await _bus.SendAsync<LoginCommand, LoginResult>(command, ct);
            return result.Success ? Ok(result) : Unauthorized(new { result.Message });
        }

        /// <summary>POST api/auth/refresh</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenCommand command, CancellationToken ct)
        {
            var result = await _bus.SendAsync<RefreshTokenCommand, RefreshTokenResult>(command, ct);
            return result.Success ? Ok(result) : Unauthorized(new { result.Message });
        }

        /// <summary>
        /// POST api/auth/logout
        /// Invalida el RefreshToken del usuario autenticado.
        /// El UserId se extrae del JWT — no se acepta en el body.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            // ── Extraer UserId del JWT (claim "uid") ──────────────────────
            int userId = User.GetUserId();
            if (userId == 0)
                return Unauthorized("Token inválido.");

            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Id == userId, ct);

            if (user == null) return NotFound();

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return Ok(new { Message = "Sesión cerrada correctamente." });
        }
    }
}
