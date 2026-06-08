// PROPÓSITO: Entidad de identidad de la plataforma.
//            Representa a cualquier usuario autenticado (Customer, Driver,
//            Merchant, BackOffice). El rol determina qué actor es.
//
// SCHEMA: sec (Security — nuevo schema para identidad)
//
// RELACIÓN CON ACTORES:
//   Un AppUser puede estar vinculado a exactamente uno de:
//   - CustomerId  → usuario es cliente
//   - DriverId    → usuario es repartidor
//   - MerchantId  → usuario es comercio
//   - null todos  → usuario es BackOffice/Admin
//
// REGLA DE ORO: AppUser NO hereda de BaseEntity porque no aplica soft-delete
//   en el sentido operativo. Se maneja con IsActive/IsLocked.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    /// <summary>
    /// Usuario de la plataforma. Vinculado a un actor operativo por FK nullable.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }

        /// <summary>Email único — usado como username de login.</summary>
        public string Email { get; set; } = null!;

        /// <summary>Hash de contraseña (BCrypt). Nunca se almacena en texto plano.</summary>
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// Rol del usuario en la plataforma.
        /// Valores: Customer, Driver, Merchant, BackOffice, Admin.
        /// </summary>
        public string Role { get; set; } = null!;

        // ─── VÍNCULOS A ACTORES (solo uno debe estar poblado) ────────────
        /// <summary>FK al Customer si el usuario es cliente.</summary>
        public int? CustomerId { get; set; }

        /// <summary>FK al Driver si el usuario es repartidor.</summary>
        public int? DriverId { get; set; }

        /// <summary>FK al Merchant si el usuario es comercio.</summary>
        public int? MerchantId { get; set; }

        // ─── ESTADO DE LA CUENTA ─────────────────────────────────────────
        /// <summary>Cuenta activa. False = suspendida por BackOffice.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Cuenta bloqueada por intentos fallidos.</summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>Contador de intentos fallidos de login.</summary>
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>Fecha hasta la que la cuenta está bloqueada (nullable).</summary>
        public DateTime? LockedUntil { get; set; }

        // ─── REFRESH TOKEN ────────────────────────────────────────────────
        /// <summary>Token de refresco activo (nullable si no hay sesión).</summary>
        public string? RefreshToken { get; set; }

        /// <summary>Fecha de expiración del refresh token.</summary>
        public DateTime? RefreshTokenExpiresAt { get; set; }

        // ─── AUDITORÍA ────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
