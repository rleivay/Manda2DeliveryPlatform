// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Payments/Commands/ConfirmPaymentCommand.cs
// PROPÓSITO:
//   Comando que el operador de BackOffice ejecuta para validar una
//   transferencia bancaria. Transiciona el pago de Pending → Confirmed
//   y el OrderGroup de PendingPayment → PaymentConfirmed → StartDispatch.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;

namespace Manda2.Application.Feature.Payments.Commands
{
    /// <summary>
    /// Comando de validación de transferencia por BackOffice.
    /// </summary>
    public class ConfirmPaymentCommand : ICommand<ConfirmPaymentResult>
    {
        /// <summary>ID del registro en fin.OrderGroupPayments a confirmar.</summary>
        public int OrderGroupPaymentId { get; }

        /// <summary>
        /// Usuario de BackOffice que valida (obtenido del JWT en el controller).
        /// Se registra en OrderGroupPayment.ConfirmedByUserId y AuditLog.
        /// </summary>
        public int ConfirmedByUserId { get; }

        /// <summary>
        /// Número de autorización bancaria confirmado con el banco.
        /// Requerido para completar la validación.
        /// </summary>
        public string Authorization { get; }

        /// <summary>
        /// Referencia bancaria final (puede diferir de la referencia inicial del cliente).
        /// </summary>
        public string? Reference { get; }

        /// <summary>
        /// Notas del operador (opcional). Ej: "Validado con Banco Industrial ref. 12345".
        /// </summary>
        public string? Notes { get; }

        public ConfirmPaymentCommand(
            int orderGroupPaymentId,
            int confirmedByUserId,
            string authorization,
            string? reference = null,
            string? notes = null)
        {
            OrderGroupPaymentId = orderGroupPaymentId;
            ConfirmedByUserId = confirmedByUserId;
            Authorization = authorization;
            Reference = reference;
            Notes = notes;
        }
    }
}