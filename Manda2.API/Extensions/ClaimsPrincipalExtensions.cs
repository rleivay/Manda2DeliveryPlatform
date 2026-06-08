// PROPÓSITO: Métodos de extensión para extraer claims del JWT en controllers.
//            Reemplaza los "TODO Sprint 4: del JWT" en todos los handlers.
//
// USO EN CONTROLLERS:
//   int driverId  = User.GetDriverId()!.Value;
//   int merchantId = User.GetMerchantId()!.Value;
//   int customerId = User.GetCustomerId()!.Value;
//   int userId    = User.GetUserId();
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Domain.Constants;
using System.Security.Claims;

namespace Manda2.API.Extensions
{
    /// <summary>
    /// Extensiones de ClaimsPrincipal para extraer IDs de actor del JWT.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>UserId del AppUser (claim "uid"). 0 si no existe.</summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(AppClaims.UserId)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        /// <summary>CustomerId (claim "cid"). Null si no existe.</summary>
        public static int? GetCustomerId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(AppClaims.CustomerId)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>DriverId (claim "did"). Null si no existe.</summary>
        public static int? GetDriverId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(AppClaims.DriverId)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>MerchantId (claim "mid"). Null si no existe.</summary>
        public static int? GetMerchantId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(AppClaims.MerchantId)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>Retorna el rol del usuario desde ClaimTypes.Role.</summary>
        public static string? GetRole(this ClaimsPrincipal principal)
            => principal.FindFirst(ClaimTypes.Role)?.Value;
    }
}
