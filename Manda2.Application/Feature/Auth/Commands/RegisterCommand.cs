// PROPÓSITO: Registro de nuevo usuario en la plataforma.
//            Crea AppUser + vincula al actor correspondiente según rol.
//
// CONSUMIDOR: AuthController → POST api/auth/register
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    public class RegisterCommand : ICommand<RegisterResult>
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        /// <summary>
        /// Rol solicitado. Valores válidos: Customer, Driver, Merchant.
        /// BackOffice/Admin solo se crean desde el panel de administración.
        /// </summary>
        public string Role { get; set; } = null!;

        // ─── Datos del actor (según rol) ─────────────────────────────────
        /// <summary>Nombre completo (Customer o Driver).</summary>
        //public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; } 

        /// <summary>Teléfono de contacto.</summary>
        public string? Phone { get; set; }

        /// <summary>Nombre del comercio (solo rol Merchant).</summary>
        public string? MerchantName { get; set; }
    }
}
