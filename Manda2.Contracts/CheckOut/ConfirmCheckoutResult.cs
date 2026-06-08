// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Payments/Commands/ConfirmCheckoutResult.cs
// PROPÓSITO:
//   Resultado del comando ConfirmCheckoutCommand.
//   Informa al cliente el nuevo estado del pedido y si el dispatch
//   fue iniciado automáticamente (Efectivo/Tarjeta) o queda pendiente
//   de validación (Transferencia).
// ════════════════════════════════════════════════════════════════════════════

namespace Manda2.Contracts.CheckOut
{
    /// <summary>
    /// Resultado del checkout. El cliente app usa estos campos para
    /// mostrar la pantalla correcta post-checkout.
    /// </summary>
    public class ConfirmCheckoutResult
    {
        /// <summary>ID del OrderGroup procesado.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>ID del registro de pago creado en fin.OrderGroupPayments.</summary>
        public int OrderGroupPaymentId { get; set; }

        /// <summary>
        /// Estado resultante del OrderGroup:
        /// - PendingPayment    → Transferencia, esperando validación BackOffice.
        /// - PaymentConfirmed  → Efectivo/Tarjeta, dispatch iniciado automáticamente.
        /// </summary>
        public string OrderGroupStatus { get; set; } = default!;

        /// <summary>
        /// Indica si el dispatch fue iniciado automáticamente.
        /// True  → Efectivo/Tarjeta (flujo inmediato).
        /// False → Transferencia (flujo manual BackOffice).
        /// </summary>
        public bool DispatchStarted { get; set; }

        /// <summary>
        /// Mensaje descriptivo para mostrar al cliente en la app.
        /// Ej: "Tu pedido está siendo asignado a un repartidor."
        ///     "Tu transferencia está siendo validada. Te notificaremos pronto."
        /// </summary>
        public string Message { get; set; } = default!;

        // ─── Factory Methods ────────────────────────────────────────────────

        public static ConfirmCheckoutResult AutoConfirmed(int groupId, int paymentId) => new()
        {
            OrderGroupId = groupId,
            OrderGroupPaymentId = paymentId,
            OrderGroupStatus = "PaymentConfirmed",
            DispatchStarted = true,
            Message = "¡Pedido confirmado! Estamos buscando un repartidor para ti."
        };

        public static ConfirmCheckoutResult PendingTransfer(int groupId, int paymentId) => new()
        {
            OrderGroupId = groupId,
            OrderGroupPaymentId = paymentId,
            OrderGroupStatus = "PendingPayment",
            DispatchStarted = false,
            Message = "Tu comprobante fue recibido. Un operador validará tu transferencia y te notificaremos."
        };
    }
}