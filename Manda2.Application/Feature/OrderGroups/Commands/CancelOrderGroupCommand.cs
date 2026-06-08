// PROPÓSITO: Contrato de entrada para cancelar un OrderGroup.
//            Puede ser invocado por el Customer (desde su app) o por BackOffice.
//            El handler valida el rol del solicitante para aplicar
//            las reglas de negocio correctas.
//
// ESTADOS CANCELABLES:
//   Draft, PendingPayment, PaymentConfirmed, AwaitingDriverAssignment
//   → Cancelación libre (sin penalización en MVP).
//   AssignedToDriver, DriverAccepted, InProgress
//   → Solo BackOffice puede cancelar (requiere liberación del driver).

using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Commands
{
    /// <summary>
    /// Comando para cancelar un OrderGroup.
    /// </summary>
    public class CancelOrderGroupCommand : ICommand<CancelOrderGroupResult>
    {
        /// <summary>ID del OrderGroup a cancelar.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>
        /// ID del usuario que solicita la cancelación.
        /// Si es el Customer → debe coincidir con OrderGroup.CustomerId.
        /// Si es BackOffice → puede cancelar cualquier grupo.
        /// </summary>
        public int RequestedByUserId { get; set; }

        /// <summary>
        /// Rol del solicitante. Valores válidos: "Customer", "BackOffice", "Admin".
        /// Determina qué estados son cancelables.
        /// </summary>
        public string RequestedByRole { get; set; } = string.Empty;

        /// <summary>
        /// Motivo de cancelación (texto libre).
        /// Obligatorio para BackOffice, opcional para Customer.
        /// Se guarda en AuditLog para trazabilidad.
        /// </summary>
        public string? CancellationReason { get; set; }
    }
}
