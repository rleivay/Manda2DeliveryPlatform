// PROPÓSITO: El comercio rechaza una SubOrder.
//            Transición: PendingMerchantAcceptance → RejectedByMerchant
//
// EFECTO EN EL GRUPO:
//   Un rechazo de cualquier comercio cancela el OrderGroup completo.
//   El cliente debe ser notificado y puede reordenar.
//
// CONSUMIDOR: MerchantsController → POST api/merchants/suborders/{id}/reject
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Commands
{
    /// <summary>
    /// Comando para que el comercio rechace una SubOrder.
    /// </summary>
    public class RejectSubOrderCommand : ICommand<RejectSubOrderResult>
    {
        /// <summary>ID de la SubOrder a rechazar.</summary>
        public int SubOrderId { get; set; }

        /// <summary>ID del Merchant que rechaza.</summary>
        public int MerchantId { get; set; }

        /// <summary>
        /// Motivo del rechazo (texto libre del comercio).
        /// Ej: "Sin stock", "Cocina cerrada", "Producto no disponible".
        /// </summary>
        public string? RejectReason { get; set; }
    }
}
