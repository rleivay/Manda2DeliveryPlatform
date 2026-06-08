//
// PROPÓSITO: Constantes de roles de la plataforma.
//            Centraliza los strings de roles para evitar typos en
//            [Authorize(Roles = "...")] y en la generación de JWT claims.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Constants
{
    /// <summary>
    /// Roles de la plataforma Manda2.
    /// Usar siempre estas constantes — nunca strings literales.
    /// </summary>
    public static class AppRoles
    {
        public const string Customer = "Customer";
        public const string Driver = "Driver";
        public const string Merchant = "Merchant";
        public const string BackOffice = "BackOffice";
        public const string Admin = "Admin";
    }
}
