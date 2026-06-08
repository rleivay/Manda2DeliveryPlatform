// PROPÓSITO: Contrato del servicio de generación y validación de JWT.
//            La implementación vive en Infrastructure para no contaminar
//            Application con dependencias de System.IdentityModel.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Common
{
    /// <summary>
    /// Servicio de tokens JWT. Implementado en Infrastructure.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Genera un Access Token JWT firmado para el usuario dado.
        /// Incluye claims: uid, email, role, cid/did/mid según actor.
        /// </summary>
        string GenerateAccessToken(AppUser user);

        /// <summary>
        /// Genera un Refresh Token opaco (GUID seguro).
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Valida un Access Token expirado y retorna el principal de claims.
        /// Usado en el flujo de refresh para extraer uid sin validar expiración.
        /// </summary>
        System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
