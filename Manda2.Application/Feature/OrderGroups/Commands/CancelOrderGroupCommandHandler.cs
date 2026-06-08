// PROPÓSITO: Handler para cancelar un OrderGroup.
//
// OPERACIONES ATÓMICAS:
//   1. Cargar OrderGroup con SubOrders, Stops y Driver asignado.
//   2. Validar ownership (Customer solo cancela sus propios grupos).
//   3. Validar estado cancelable según rol del solicitante.
//   4. Cancelar SubOrders activas (estados no terminales).
//   5. Cancelar el OrderGroup → Status = Cancelled.
//   6. Liberar al driver si estaba asignado (CurrentOrderGroupId = null,
//      Status = Available).
//   7. AuditLog con RequiresImmediateAttention si había driver asignado.
//   8. Notificar al cliente y comercios vía INotificationService.
//   9. Persistir en una sola transacción.

using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manda2.Contracts.CheckOut;

namespace Manda2.Application.Feature.OrderGroups.Commands
{
    /// <summary>
    /// Handler para <see cref="CancelOrderGroupCommand"/>.
    /// </summary>
    public class CancelOrderGroupCommandHandler
        : ICommandHandler<CancelOrderGroupCommand, CancelOrderGroupResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly INotificationService _notifications;

        // Estados que NO son terminales — pueden ser cancelados
        private static readonly OrderGroupStatus[] CancelableByCustomer =
        {
            OrderGroupStatus.Draft,
            OrderGroupStatus.PendingPayment,
            OrderGroupStatus.PaymentConfirmed,
            OrderGroupStatus.AwaitingDriverAssignment
        };

        // BackOffice puede cancelar estos estados adicionales
        private static readonly OrderGroupStatus[] CancelableByBackOffice =
        {
            OrderGroupStatus.Draft,
            OrderGroupStatus.PendingPayment,
            OrderGroupStatus.PaymentConfirmed,
            OrderGroupStatus.AwaitingDriverAssignment,
            OrderGroupStatus.AssignedToDriver,
            OrderGroupStatus.DriverAccepted,
            OrderGroupStatus.CapacityValidated,
            OrderGroupStatus.InRoute
        };

        // SubOrder states que pueden ser cancelados (no terminales)
        private static readonly SubOrderStatus[] CancelableSubOrderStatuses =
        {
            SubOrderStatus.PendingMerchantAcceptance,
            SubOrderStatus.AcceptedByMerchant,
            SubOrderStatus.Preparing,
            SubOrderStatus.ReadyForPickup
        };

        public CancelOrderGroupCommandHandler(
            IApplicationDbContext db,
            INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<CancelOrderGroupResult> HandleAsync(
            CancelOrderGroupCommand command, CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Cargar OrderGroup con SubOrders y Driver ────
            var group = await _db.OrderGroups
                .Include(og => og.SubOrders)
                .FirstOrDefaultAsync(og => og.Id == command.OrderGroupId && !og.IsDeleted, ct);

            if (group == null)
                return Fail("El pedido no existe.");

            // ── PASO 2: Validar ownership (Customer solo cancela los suyos) ────
            bool isBackOffice = command.RequestedByRole == "BackOffice"
                             || command.RequestedByRole == "Admin";

            if (!isBackOffice && group.CustomerId != command.RequestedByUserId)
                return Fail("No tienes permiso para cancelar este pedido.");

            // ── PASO 3: Validar estado cancelable según rol ────
            var allowedStatuses = isBackOffice ? CancelableByBackOffice : CancelableByCustomer;

            if (!allowedStatuses.Contains(group.Status))
                return Fail($"El pedido en estado '{group.Status}' no puede ser cancelado " +
                            $"por {command.RequestedByRole}.");

            // ── PASO 4: Cancelar SubOrders activas ────
            int cancelledSubOrders = 0;
            foreach (var sub in group.SubOrders.Where(s => CancelableSubOrderStatuses.Contains(s.Status)))
            {
                sub.Status = SubOrderStatus.Cancelled;
                sub.CancelledAt = utcNow;
                cancelledSubOrders++;
            }

            // ── PASO 5: Cancelar el OrderGroup ────
            group.Status = OrderGroupStatus.Cancelled;
            group.UpdatedAt = utcNow;

            // ── PASO 6: Liberar al driver si estaba asignado ────
            bool driverReleased = false;

            if (group.DriverId.HasValue)
            {
                var driver = await _db.Drivers
                    .FirstOrDefaultAsync(d => d.Id == group.DriverId.Value, ct);

                if (driver != null)
                {
                    driver.CurrentOrderGroupId = null;
                    driver.Status = DriverStatus.Available;
                    driverReleased = true;
                }
            }

            // ── PASO 7: AuditLog ────
            // RequiresImmediateAttention = true si había driver asignado
            // (cancelación tardía — puede impactar métricas del driver).
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                Action = "CancelOrderGroup",
                PerformedByUserId = command.RequestedByUserId,
                Details = $"Cancelado por {command.RequestedByRole} " +
                                           $"(UserId:{command.RequestedByUserId}). " +
                                           $"Motivo: {command.CancellationReason ?? "No especificado"}. " +
                                           $"SubOrders canceladas: {cancelledSubOrders}. " +
                                           $"Driver liberado: {driverReleased}.",
                RequiresImmediateAttention = driverReleased,
                CreatedAt = utcNow
            });

            // ── PASO 8: Persistir en una sola transacción ────
            await _db.SaveChangesAsync(ct);

            // ── PASO 9: Notificar (post-save, best-effort) ────
            // Si la notificación falla, el estado en BD ya es correcto.
            var merchantIds = group.SubOrders.Select(s => s.MerchantId).Distinct();

            await _notifications.NotifyOrderGroupStatusChangedAsync(
                orderGroupId: group.Id,
                newStatus: OrderGroupStatus.Cancelled.ToString(),
                customerId: group.CustomerId,
                merchantIds: merchantIds,
                cancellationToken: ct);

            return new CancelOrderGroupResult
            {
                Success = true,
                Message = $"Pedido cancelado exitosamente.",
                DriverReleased = driverReleased,
                SubOrdersCancelled = cancelledSubOrders
            };
        }

        private static CancelOrderGroupResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
