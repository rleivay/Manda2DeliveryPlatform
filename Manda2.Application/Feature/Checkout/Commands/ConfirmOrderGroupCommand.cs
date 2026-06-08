using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// PROPÓSITO:
//   Comando de consolidación logística del checkout (Opción B — Fase 2).
//   Recibe la selección de dirección de entrega del cliente y:
//     1. Valida ownership de ShippingAddress.
//     2. Snapshot de dirección en OrderGroup.
//     3. Calcula DeliveryFee por Haversine (Merchant → ShippingAddress).
//     4. Proratea el fee entre SubOrders.
//     5. Actualiza SubOrders con campos de auditoría de delivery.
//     6. Actualiza Stop Dropoff con la dirección real.
//     7. Transiciona OrderGroup: Draft → CapacityValidated.
//     8. Registra AuditLog.
//
//   FLUJO COMPLETO:
//     POST /api/checkout/create-order-group  → Draft
//     POST /api/checkout/confirm-order-group → CapacityValidated  ← ESTE COMANDO
//     POST /api/checkout                     → PendingPayment / PaymentConfirmed
//
// PATRÓN: ICommand<TResult> — mediador propio Manda2.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;

namespace Manda2.Application.Feature.Checkout.Commands
{
    /// <summary>
    /// Consolida la dirección de entrega y calcula el delivery fee real.
    /// Transiciona el OrderGroup de Draft → CapacityValidated.
    /// </summary>
    public class ConfirmOrderGroupCommand : ICommand<ConfirmOrderGroupResult>
    {
        /// <summary>ID del OrderGroup en estado Draft a consolidar.</summary>
        public int OrderGroupId { get; }

        /// <summary>
        /// ID del cliente autenticado (obtenido del JWT en el controller).
        /// Usado para validar ownership del OrderGroup y de la ShippingAddress.
        /// </summary>
        public int CustomerId { get; }

        /// <summary>
        /// ID de la ShippingAddress seleccionada por el cliente.
        /// Debe pertenecer al CustomerId y estar activa.
        /// </summary>
        public int ShippingAddressId { get; }

        public ConfirmOrderGroupCommand(
            int orderGroupId,
            int customerId,
            int shippingAddressId)
        {
            OrderGroupId = orderGroupId;
            CustomerId = customerId;
            ShippingAddressId = shippingAddressId;
        }
    }
}
