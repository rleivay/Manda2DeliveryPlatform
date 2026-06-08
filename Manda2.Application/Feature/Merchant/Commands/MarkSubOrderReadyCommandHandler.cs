// PROPÓSITO: Lógica de negocio para marcar SubOrder como ReadyForPickup.
//
// TRANSICIONES VÁLIDAS:
//   AcceptedByMerchant → Preparing → ReadyForPickup
//   AcceptedByMerchant → ReadyForPickup (si el comercio no usa estado Preparing)
//
// OPERACIONES ATÓMICAS:
//   1. Validar existencia, ownership y estado.
//   2. Marcar ReadyForPickup + ReadyAt.
//   3. Evaluar si TODAS las SubOrders del grupo están listas.
//   4. AuditLog.
//   5. Persistir.

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
    /// Handler del comando MarkSubOrderReadyCommand.
    /// </summary>
    public class MarkSubOrderReadyCommandHandler
        : ICommandHandler<MarkSubOrderReadyCommand, MarkSubOrderReadyResult>
    {
        private readonly IApplicationDbContext _db;

        public MarkSubOrderReadyCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<MarkSubOrderReadyResult> HandleAsync(
            MarkSubOrderReadyCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar SubOrder con hermanas del grupo.
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
                return Fail("No tienes permiso para actualizar esta suborden.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar transición de estado.
            //
            // Transiciones válidas hacia ReadyForPickup:
            //   - AcceptedByMerchant (comercio sin estado Preparing)
            //   - Preparing (comercio que usa el estado intermedio)
            // ─────────────────────────────────────────────────────────────
            var validPreviousStatuses = new[]
            {
                SubOrderStatus.AcceptedByMerchant,
                SubOrderStatus.Preparing
            };

            if (!validPreviousStatuses.Contains(subOrder.Status))
                return Fail($"No se puede marcar como lista desde el estado: {subOrder.Status}.");

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Marcar como ReadyForPickup.
            // ─────────────────────────────────────────────────────────────
            subOrder.Status = SubOrderStatus.ReadyForPickup;
            subOrder.ReadyAt = DateTime.UtcNow;

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Evaluar si TODAS las SubOrders activas están listas.
            //
            // "Activas" = no Cancelled, no Rejected.
            // Útil para notificar al driver que puede iniciar la ruta.
            // ─────────────────────────────────────────────────────────────
            var activeSubOrders = subOrder.OrderGroup.SubOrders
                .Where(s => s.Status != SubOrderStatus.Cancelled
                         && s.Status != SubOrderStatus.RejectedByMerchant)
                .ToList();

            bool allReady = activeSubOrders
                .All(s => s.Status == SubOrderStatus.ReadyForPickup);

            // ─────────────────────────────────────────────────────────────
            // PASO 6: AuditLog.
            // ─────────────────────────────────────────────────────────────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(SubOrder),
                EntityId = subOrder.Id,
                Action = "MarkSubOrderReady",
                PerformedByUserId = command.MerchantId,
                //PerformedByRole = "Merchant",
                Details = $"SubOrder {subOrder.Id} marcada ReadyForPickup por Merchant {command.MerchantId}." +
                                          (allReady ? " Todos los comercios del grupo están listos." : ""),
                RequiresImmediateAttention = false,
                CreatedAt = DateTime.UtcNow
            });

            // ─────────────────────────────────────────────────────────────
            // PASO 7: Persistir.
            // ─────────────────────────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // TODO Sprint 4: Si allReady → notificar al driver vía SignalR
            // que todos los comercios están listos para pickup.

            return new MarkSubOrderReadyResult
            {
                Success = true,
                AllSubOrdersReady = allReady,
                Message = allReady
                    ? "Pedido listo. Todos los comercios confirmaron preparación. El repartidor puede iniciar la ruta."
                    : "Pedido marcado como listo. Esperando a otros comercios del grupo."
            };
        }

        private static MarkSubOrderReadyResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
