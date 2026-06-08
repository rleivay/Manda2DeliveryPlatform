// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Payments/Commands/ConfirmCheckoutCommandHandler.cs
// PROPÓSITO:
//   Orquesta el flujo completo de checkout:
//   1. Valida que el OrderGroup exista, pertenezca al cliente y esté en Draft.
//   2. Valida que el monto cubra el TotalAmount del grupo.
//   3. Crea el registro en fin.OrderGroupPayments con estado Pending.
//   4. Actualiza PaymentMethodId/Name en OrderGroup (desnormalizado para driver).
//   5. Transiciona el estado:
//      - Efectivo/Tarjeta → PaymentConfirmed → invoca StartDispatchCommand.
//      - Transferencia    → PendingPayment   → espera validación BackOffice.
//   6. Registra en aud.AuditLogs.
//
// PATRÓN: ICommandHandler<TCommand, TResult> — mediador propio Manda2.
// DEPENDENCIAS: IApplicationDbContext, ICommandBus (para StartDispatch).
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
    /// <summary>
    /// Handler del comando ConfirmCheckoutCommand.
    /// Punto de entrada del flujo de pago desde la app del cliente.
    /// </summary>
    public class ConfirmCheckoutCommandHandler
        : ICommandHandler<ConfirmCheckoutCommand, ConfirmCheckoutResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICommandBus _bus;

        // ─── Códigos de método de pago (configurables en cfg.PaymentMethods) ────
        // IMPORTANTE: estos valores deben coincidir con el campo "Code" de la tabla
        // cfg.PaymentMethods. Si el equipo cambia los códigos en BD, actualizar aquí
        // o mover a AppConfig para hacerlo 100% configurable.
        private const string CODE_TRANSFER = "TRANSFER";
        private const string CODE_CASH = "CASH";
        private const string CODE_CARD = "CARD";

        public ConfirmCheckoutCommandHandler(
            IApplicationDbContext db,
            ICommandBus bus)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task<ConfirmCheckoutResult> HandleAsync(
            ConfirmCheckoutCommand request,
            CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar OrderGroup con método de pago ─────────────────
            var group = await _db.OrderGroups
                .Include(og => og.PaymentMethod)
                .FirstOrDefaultAsync(og => og.Id == request.OrderGroupId, ct);

            if (group == null)
                throw new InvalidOperationException(
                    $"OrderGroup {request.OrderGroupId} no encontrado.");

            // ── PASO 2: Validar propiedad del cliente ────────────────────────
            if (group.CustomerId != request.CustomerId)
                throw new UnauthorizedAccessException(
                    $"El cliente {request.CustomerId} no es propietario del grupo {request.OrderGroupId}.");

            // ── PASO 3: Validar estado — solo Draft puede hacer checkout ─────
            if (group.Status != OrderGroupStatus.CapacityValidated)
                throw new InvalidOperationException(
                    $"El OrderGroup {request.OrderGroupId} no está en estado CapacityValidated. " +
                    $"Estado actual: {group.Status}. " +
                    $"Debe confirmar la dirección de entrega antes de proceder al pago.");

            // ── PASO 4: Validar cobertura del monto ──────────────────────────
            // El monto declarado debe cubrir el total del grupo.
            // Para split payment futuro, esta validación se relajará.
            if (request.Amount < group.TotalAmount)
                throw new InvalidOperationException(
                    $"El monto declarado ({request.Amount:C}) es menor al total del pedido ({group.TotalAmount:C}).");

            // ── PASO 5: Cargar método de pago para obtener su Code ───────────
            var paymentMethod = await _db.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId, ct);

            if (paymentMethod == null)
                throw new InvalidOperationException(
                    $"Método de pago {request.PaymentMethodId} no encontrado.");

            // ── PASO 6: Validar comprobante para Transferencia ───────────────
            bool isTransfer = paymentMethod.Code?.ToUpperInvariant() == CODE_TRANSFER;

            if (isTransfer && string.IsNullOrWhiteSpace(request.ProofUrl))
                throw new InvalidOperationException(
                    "Para pagos por Transferencia es obligatorio adjuntar el comprobante (ProofUrl).");

            // ── PASO 7: Crear registro de pago en fin.OrderGroupPayments ─────
            var payment = new OrderGroupPayment
            {
                OrderGroupId = group.Id,
                PaymentMethodId = request.PaymentMethodId,
                Total = request.Amount,
                Reference = request.Reference,
                ProofUrl = request.ProofUrl,
                // Transferencia → Pending (BackOffice valida)
                // Efectivo/Tarjeta → Confirmed automáticamente
                Status = isTransfer ? PaymentStatus.Pending : PaymentStatus.Confirmed,
                CreatedAt = utcNow
            };

            _db.OrderGroupPayments.Add(payment);

            // ── PASO 8: Desnormalizar método de pago en OrderGroup ───────────
            // El driver necesita ver el método de pago sin joins adicionales.
            group.PaymentMethodId = request.PaymentMethodId;
            group.PaymentMethodName = paymentMethod.Name;

            // ── PASO 9: Transición de estado ─────────────────────────────────
            bool autoConfirm = !isTransfer; // Efectivo y Tarjeta se auto-confirman

            if (autoConfirm)
            {
                group.Status = OrderGroupStatus.PaymentConfirmed;
            }
            else
            {
                // Transferencia: queda en PendingPayment hasta validación BackOffice
                group.Status = OrderGroupStatus.PendingPayment;
            }

            // ── PASO 10: AuditLog ────────────────────────────────────────────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                Action = "ConfirmCheckout",
                PerformedByUserId = request.CustomerId,
                Details = $"Método: {paymentMethod.Name} | Monto: {request.Amount:C} | " +
                                       $"Estado resultante: {group.Status} | AutoConfirm: {autoConfirm}",
                Category = "CHECKOUT",
                CreatedAt = utcNow
            });

            // ── PASO 11: Persistir ───────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // ── PASO 12: Iniciar Dispatch automáticamente si aplica ──────────
            // Solo para Efectivo/Tarjeta. Transferencia espera BackOffice.
            if (autoConfirm)
            {
                await _bus.SendAsync<StartDispatchCommand, StartDispatchResult>(
                    new StartDispatchCommand(
                        orderGroupId: group.Id,
                        initiatedByUserId: request.CustomerId,
                        zoneName: null   // Usa configuración por defecto de AppConfig
                    ), ct);

                return ConfirmCheckoutResult.AutoConfirmed(group.Id, payment.Id);
            }

            return ConfirmCheckoutResult.PendingTransfer(group.Id, payment.Id);
        }
    }
}