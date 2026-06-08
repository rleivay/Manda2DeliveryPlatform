// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Payments/Commands/ConfirmPaymentResult.cs
// ════════════════════════════════════════════════════════════════════════════

namespace Manda2.Contracts.CheckOut
{
    public class ConfirmPaymentResult
    {
        public int OrderGroupId { get; set; }
        public int OrderGroupPaymentId { get; set; }
        public bool DispatchStarted { get; set; }
        public string Message { get; set; } = default!;

        public static ConfirmPaymentResult Success(int groupId, int paymentId) => new()
        {
            OrderGroupId = groupId,
            OrderGroupPaymentId = paymentId,
            DispatchStarted = true,
            Message = "Transferencia validada. Dispatch iniciado."
        };
    }
}