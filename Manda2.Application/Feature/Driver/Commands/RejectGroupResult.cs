// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/RejectGroup/
//          RejectGroupResult.cs
//
// PROPÓSITO: Define el contrato de salida del comando RejectGroup.
//            La app del driver usa este resultado para confirmar el rechazo
//            y limpiar la tarjeta de oferta de la pantalla.
//
// NOTA: Intencionalmente simple. No retorna datos del grupo porque
//       el driver ya no tiene relación con él después del rechazo.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Resultado del comando RejectGroupCommand.
    /// </summary>
    public class RejectGroupResult
    {
        /// <summary>
        /// Indica si el rechazo fue procesado exitosamente.
        /// False si la oferta ya expiró o fue procesada por otro proceso.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado.
        /// Se puede mostrar directamente en la app del driver.
        /// </summary>
        public string Message { get; set; } = null!;
    }
}
