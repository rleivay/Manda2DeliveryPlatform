// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Payments/Commands/ConfirmPaymentCommandHandler.cs
// PROPÓSITO:
//   Handler que procesa la validación de transferencia por BackOffice.
//   Flujo:
//   1. Carga el OrderGroupPayment y su OrderGroup.
//   2. Valida que el pago esté en Pending y el grupo en PendingPayment.
//   3. Confirma el pago: Authorization, ConfirmedByUserId, ConfirmedAt.
//   4. Verifica cobertura total (suma de pagos confirmados >= TotalAmount).
//   5. Transiciona OrderGroup → PaymentConfirmed.
//   6. Invoca StartDispatchCommand vía ICommandBus.
//   7. Registra en AuditLog.
//
// OVERPAYMENT: Si la suma supera TotalAmount, se registra AuditLog con
//   RequiresImmediateAttention = true para revisión de BackOffice.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using Manda2.Contracts.CheckOut;

namespace Manda2.Application.Feature.Payments.Commands
{
    public class ConfirmPaymentCommandHandler
        : ICommandHandler<ConfirmPaymentCommand, ConfirmPaymentResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICommandBus _bus;

        public ConfirmPaymentCommandHandler(
            IApplicationDbContext db,
            ICommandBus bus)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task<ConfirmPaymentResult> HandleAsync(
            ConfirmPaymentCommand request,
            CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar pago con su OrderGroup ────────────────────────
            var payment = await _db.OrderGroupPayments
                .Include(p => p.OrderGroup)
                .FirstOrDefaultAsync(p => p.Id == request.OrderGroupPaymentId, ct);

            if (payment == null)
                throw new InvalidOperationException(
                    $"OrderGroupPayment {request.OrderGroupPaymentId} no encontrado.");

            var group = payment.OrderGroup
                ?? throw new InvalidOperationException(
                    $"OrderGroup no encontrado para el pago {request.OrderGroupPaymentId}.");

            // ── PASO 2: Validar estado del pago ─────────────────────────────
            if (payment.Status != PaymentStatus.Pending)
                throw new InvalidOperationException(
                    $"El pago {payment.Id} no está en estado Pending. " +
                    $"Estado actual: {payment.Status}. No se puede confirmar.");

            // ── PASO 3: Validar estado del OrderGroup ────────────────────────
            if (group.Status != OrderGroupStatus.PendingPayment)
                throw new InvalidOperationException(
                    $"El OrderGroup {group.Id} no está en PendingPayment. " +
                    $"Estado actual: {group.Status}.");

            // ── PASO 4: Confirmar el pago ────────────────────────────────────
            payment.Status = PaymentStatus.Confirmed;
            payment.Authorization = request.Authorization;
            payment.ConfirmedByUserId = request.ConfirmedByUserId;
            payment.ConfirmedAt = utcNow;

            // Actualizar referencia si el operador la corrige
            if (!string.IsNullOrWhiteSpace(request.Reference))
                payment.Reference = request.Reference;

            // ── PASO 5: Verificar cobertura total ────────────────────────────
            // Suma todos los pagos confirmados del grupo (incluye el que acabamos de confirmar)
            var confirmedTotal = await _db.OrderGroupPayments
                .Where(p => p.OrderGroupId == group.Id
                         && p.Status == PaymentStatus.Confirmed
                         && p.Id != payment.Id) // Excluir el actual (aún no guardado)
                .SumAsync(p => p.Total, ct);

            confirmedTotal += payment.Total; // Agregar el pago actual

            // ── PASO 6: Detectar overpayment ─────────────────────────────────
            if (confirmedTotal > group.TotalAmount)
            {
                _db.AuditLogs.Add(new AuditLog
                {
                    EntityName = nameof(OrderGroupPayment),
                    EntityId = payment.Id,
                    Action = "OverpaymentDetected",
                    PerformedByUserId = request.ConfirmedByUserId,
                    Details = $"Grupo {group.Id}: Total={group.TotalAmount:C} | " +
                                                $"Pagado={confirmedTotal:C} | " +
                                                $"Exceso={(confirmedTotal - group.TotalAmount):C}",
                    Category = "OVERPAYMENT",
                    RequiresImmediateAttention = true,
                    CreatedAt = utcNow
                });
            }

            // ── PASO 7: Transición de estado del OrderGroup ──────────────────
            // Solo avanza si la cobertura es suficiente
            if (confirmedTotal >= group.TotalAmount)
            {
                group.Status = OrderGroupStatus.PaymentConfirmed;
            }
            else
            {
                // Pago parcial: queda en PendingPayment esperando más pagos
                // (split payment futuro — por ahora no debería ocurrir en MVP)
                _db.AuditLogs.Add(new AuditLog
                {
                    EntityName = nameof(OrderGroup),
                    EntityId = group.Id,
                    Action = "PartialPaymentConfirmed",
                    PerformedByUserId = request.ConfirmedByUserId,
                    Details = $"Confirmado parcial: {confirmedTotal:C} de {group.TotalAmount:C}",
                    Category = "PAYMENT",
                    CreatedAt = utcNow
                });

                await _db.SaveChangesAsync(ct);

                // No iniciamos dispatch — falta cobertura
                return new ConfirmPaymentResult
                {
                    OrderGroupId = group.Id,
                    OrderGroupPaymentId = payment.Id,
                    DispatchStarted = false,
                    Message = $"Pago parcial confirmado. Pendiente: {(group.TotalAmount - confirmedTotal):C}."
                };
            }

            // ── PASO 8: AuditLog de confirmación exitosa ─────────────────────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroupPayment),
                EntityId = payment.Id,
                Action = "PaymentConfirmed",
                PerformedByUserId = request.ConfirmedByUserId,
                Details = $"Grupo {group.Id} | Auth: {request.Authorization} | " +
                                    $"Total confirmado: {confirmedTotal:C} | Notas: {request.Notes}",
                Category = "PAYMENT",
                CreatedAt = utcNow
            });

            // ── PASO 9: Persistir ────────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // ── PASO 10: Iniciar Dispatch ────────────────────────────────────
            await _bus.SendAsync<StartDispatchCommand, StartDispatchResult>(
                new StartDispatchCommand(
                    orderGroupId: group.Id,
                    initiatedByUserId: request.ConfirmedByUserId,
                    zoneName: null
                ), ct);

            return ConfirmPaymentResult.Success(group.Id, payment.Id);
        }
    }
}