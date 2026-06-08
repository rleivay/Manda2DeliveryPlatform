// PROPÓSITO: Lógica de negocio para rechazo de SubOrder por el comercio.
//
// OPERACIONES ATÓMICAS:
//   1. Validar existencia, ownership y estado.
//   2. Marcar SubOrder como RejectedByMerchant + RejectedAt.
//   3. Cancelar TODAS las SubOrders activas del grupo.
//   4. Cancelar el OrderGroup.
//   5. AuditLog con RequiresImmediateAttention = true.
//   6. Persistir en una sola transacción.
//
// DECISIÓN DE DISEÑO:
//   Un rechazo parcial cancela el grupo completo (MVP).
//   En fases posteriores se puede implementar "reordenar sin ese comercio".
// ═══════════════════════════════════════════════════════════════════════════
//
using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Commands
{
    /// <summary>
    /// Handler del comando RejectSubOrderCommand.
    /// </summary>
    public class RejectSubOrderCommandHandler
        : ICommandHandler<RejectSubOrderCommand, RejectSubOrderResult>
    {
        private readonly IApplicationDbContext _db;

        public RejectSubOrderCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RejectSubOrderResult> HandleAsync(
            RejectSubOrderCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar SubOrder con todas las hermanas del grupo.
            // ─────────────────────────────────────────────────────────────
            var subOrder = await _db.SubOrders
                .Include(s => s.OrderGroup)
                    .ThenInclude(og => og.SubOrders)
                .FirstOrDefaultAsync(s => s.Id == command.SubOrderId, ct);

            if (subOrder == null)
                return Fail("La suborden no existe.");

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Validar ownership.
            // ─────────────────────────────────────────────────────────────
            if (subOrder.MerchantId != command.MerchantId)
                return Fail("No tienes permiso para rechazar esta suborden.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar estado. Solo rechaza si está pendiente.
            // ─────────────────────────────────────────────────────────────
            if (subOrder.Status != SubOrderStatus.PendingMerchantAcceptance)
                return Fail($"La suborden no está pendiente de aceptación. Estado actual: {subOrder.Status}.");

            var now = DateTime.UtcNow;
            var og = subOrder.OrderGroup;

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Rechazar la SubOrder que disparó el rechazo.
            // ─────────────────────────────────────────────────────────────
            subOrder.Status = SubOrderStatus.RejectedByMerchant;
            subOrder.RejectedAt = now;

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Cancelar todas las SubOrders activas del grupo.
            //
            // No cancelamos las que ya están en estados terminales
            // (Delivered, Cancelled) para no corromper historial.
            // ─────────────────────────────────────────────────────────────
            var cancelableStatuses = new[]
            {
                SubOrderStatus.PendingMerchantAcceptance,
                SubOrderStatus.AcceptedByMerchant,
                SubOrderStatus.Preparing
            };

            foreach (var sibling in og.SubOrders
                .Where(s => s.Id != subOrder.Id
                         && cancelableStatuses.Contains(s.Status)))
            {
                sibling.Status = SubOrderStatus.Cancelled;
                sibling.CancelledAt = now;
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 6: Cancelar el OrderGroup.
            // ─────────────────────────────────────────────────────────────
            og.Status = OrderGroupStatus.Cancelled;

            // ─────────────────────────────────────────────────────────────
            // PASO 7: AuditLog — RequiresImmediateAttention = true.
            //         BackOffice debe revisar rechazos de comercios para
            //         detectar patrones (stock, horarios, etc.).
            // ─────────────────────────────────────────────────────────────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(SubOrder),
                EntityId = subOrder.Id,
                Action = "RejectSubOrder",
                PerformedByUserId = command.MerchantId,
                //PerformedByRole = "Merchant",
                Details = $"SubOrder {subOrder.Id} rechazada por Merchant {command.MerchantId}. " +
                                             $"Motivo: {command.RejectReason ?? "No especificado"}. " +
                                             $"OrderGroup {og.Id} cancelado.",
                RequiresImmediateAttention = true,
                CreatedAt = now
            });

            // ─────────────────────────────────────────────────────────────
            // PASO 8: Persistir en una sola transacción.
            // ─────────────────────────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // TODO Sprint 4: Notificar al cliente vía INotificationService
            // que su pedido fue cancelado por rechazo del comercio.

            return new RejectSubOrderResult
            {
                Success = true,
                Message = $"SubOrder rechazada. El pedido completo fue cancelado.",
                OrderGroupCancelled = true
            };
        }

        private static RejectSubOrderResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
