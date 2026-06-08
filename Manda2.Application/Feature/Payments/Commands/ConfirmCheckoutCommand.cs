// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Payments/Commands/ConfirmCheckoutCommand.cs
// PROPÓSITO:
//   Comando que el cliente ejecuta al confirmar su carrito y seleccionar
//   método de pago. Transiciona el OrderGroup de Draft → PendingPayment
//   y crea el registro inicial en fin.OrderGroupPayments.
//
//   Flujo posterior:
//   - Efectivo/Tarjeta → PaymentConfirmed automático → StartDispatch
//   - Transferencia    → queda en PendingPayment hasta validación BackOffice
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;

namespace Manda2.Application.Feature.Payments.Commands
{
    /// <summary>
    /// Comando de checkout: el cliente confirma su pedido y elige cómo pagar.
    /// </summary>
    public class ConfirmCheckoutCommand : ICommand<ConfirmCheckoutResult>
    {
        /// <summary>
        /// ID del OrderGroup en estado Draft que se va a confirmar.
        /// </summary>
        public int OrderGroupId { get; }

        /// <summary>
        /// ID del cliente que realiza el checkout (obtenido del JWT en el controller).
        /// </summary>
        public int CustomerId { get; }

        /// <summary>
        /// ID del método de pago seleccionado (FK a cfg.PaymentMethods).
        /// Ej: 1=Efectivo, 2=Tarjeta, 3=Transferencia.
        /// </summary>
        public int PaymentMethodId { get; }

        /// <summary>
        /// Monto que el cliente declara pagar con este método.
        /// Debe ser >= OrderGroup.TotalAmount para métodos de pago único.
        /// Para split payment futuro, puede ser parcial.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// URL del comprobante de transferencia (solo requerido si PaymentMethod = Transferencia).
        /// Null para Efectivo y Tarjeta.
        /// </summary>
        public string? ProofUrl { get; }

        /// <summary>
        /// Referencia externa del pago (número de transacción, referencia bancaria, etc.).
        /// Opcional en Efectivo. Requerido en Transferencia.
        /// </summary>
        public string? Reference { get; }

        public ConfirmCheckoutCommand(
            int orderGroupId,
            int customerId,
            int paymentMethodId,
            decimal amount,
            string? proofUrl = null,
            string? reference = null)
        {
            OrderGroupId = orderGroupId;
            CustomerId = customerId;
            PaymentMethodId = paymentMethodId;
            Amount = amount;
            ProofUrl = proofUrl;
            Reference = reference;
        }
    }
}