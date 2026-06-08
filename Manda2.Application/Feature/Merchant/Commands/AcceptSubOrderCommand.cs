// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Merchant/Commands/AcceptSubOrder/
//          AcceptSubOrderCommand.cs
//
// PROPÓSITO: El comercio acepta una SubOrder asignada a él.
//            Transición: PendingMerchantAcceptance → AcceptedByMerchant
//
// CONSUMIDOR: MerchantsController → POST api/merchants/suborders/{id}/accept
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
    /// Comando para que el comercio acepte una SubOrder pendiente.
    /// </summary>
    public class AcceptSubOrderCommand : ICommand<AcceptSubOrderResult>
    {
        /// <summary>ID de la SubOrder a aceptar.</summary>
        public int SubOrderId { get; set; }

        /// <summary>
        /// ID del Merchant que acepta.
        /// En Sprint 4 se extrae del JWT. Por ahora viene del body.
        /// </summary>
        public int MerchantId { get; set; }
    }
}
