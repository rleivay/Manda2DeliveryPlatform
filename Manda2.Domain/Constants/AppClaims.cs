// PROPÓSITO: Constantes de claim types personalizados del JWT.
//            Además de los claims estándar (sub, email, role),
//            el token incluye el ID del actor operativo vinculado.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Constants
{
    /// <summary>
    /// Claim types personalizados del JWT de Manda2.
    /// </summary>
    public static class AppClaims
    {
        /// <summary>ID del AppUser. Presente en todos los tokens.</summary>
        public const string UserId = "uid";

        /// <summary>Rol del usuario (Customer, Driver, Merchant, BackOffice, Admin).</summary>
        public const string Role = "role";

        /// <summary>ID del Customer. Solo en tokens de rol Customer.</summary>
        public const string CustomerId = "cid";

        /// <summary>ID del Driver. Solo en tokens de rol Driver.</summary>
        public const string DriverId = "did";

        /// <summary>ID del Merchant. Solo en tokens de rol Merchant.</summary>
        public const string MerchantId = "mid";
    }
}
