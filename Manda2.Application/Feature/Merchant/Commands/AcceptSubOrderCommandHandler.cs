// PROPÓSITO: Lógica de negocio para aceptación de SubOrder por el comercio.
//
// OPERACIONES ATÓMICAS:
//   1. Validar que la SubOrder existe y pertenece al Merchant.
//   2. Validar que está en estado PendingMerchantAcceptance.
//   3. Marcar AcceptedByMerchant + AcceptedAt.
//   4. Evaluar si TODAS las SubOrders del grupo están aceptadas.
//   5. Si todas aceptadas → OrderGroup avanza a AwaitingDriverAssignment
//      y se dispara StartDispatch.
//   6. Registrar AuditLog.
//   7. Persistir en una sola transacción.
//
// REGLA DE NEGOCIO:
//   El dispatch solo inicia cuando TODOS los comercios del grupo aceptaron.
//   Un solo rechazo bloquea el flujo (ver RejectSubOrderCommand).
// ═══════════════════════════════════════════════════════════════════════════


using Manda2.Application.Common;
using Manda2.Application.Feature.Dispatch.Commands;
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
    /// Handler del comando AcceptSubOrderCommand.
    /// </summary>
    public class AcceptSubOrderCommandHandler
        : ICommandHandler<AcceptSubOrderCommand, AcceptSubOrderResult>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICommandBus _bus;

        public AcceptSubOrderCommandHandler(
            IApplicationDbContext db,
            ICommandBus bus)
        {
            _db = db;
            _bus = bus;
        }

        public async Task<AcceptSubOrderResult> HandleAsync(
            AcceptSubOrderCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar la SubOrder con su OrderGroup y todas las
            //         SubOrders hermanas para evaluar el avance del grupo.
            // ─────────────────────────────────────────────────────────────
            var subOrder = await _db.SubOrders
                .Include(s => s.OrderGroup)
                    .ThenInclude(og => og.SubOrders)
                .FirstOrDefaultAsync(s => s.Id == command.SubOrderId, ct);

            if (subOrder == null)
                return Fail("La suborden no existe.");

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Validar ownership — solo el comercio dueño acepta.
            // ─────────────────────────────────────────────────────────────
            if (subOrder.MerchantId != command.MerchantId)
                return Fail("No tienes permiso para aceptar esta suborden.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar estado. Solo acepta si está pendiente.
            // ─────────────────────────────────────────────────────────────
            if (subOrder.Status != SubOrderStatus.PendingMerchantAcceptance)
                return Fail($"La suborden no está pendiente de aceptación. Estado actual: {subOrder.Status}.");

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Aceptar la SubOrder.
            // ─────────────────────────────────────────────────────────────
            subOrder.Status = SubOrderStatus.AcceptedByMerchant;
            subOrder.AcceptedAt = DateTime.UtcNow;

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Evaluar si TODAS las SubOrders del grupo aceptaron.
            //
            // Incluimos la suborden actual (ya mutada en memoria).
            // Solo evaluamos las que NO están Cancelled.
            // ─────────────────────────────────────────────────────────────
            var og = subOrder.OrderGroup;

            var activeSubOrders = og.SubOrders
                .Where(s => s.Status != SubOrderStatus.Cancelled)
                .ToList();

            bool allAccepted = activeSubOrders
                .All(s => s.Status == SubOrderStatus.AcceptedByMerchant
                       || s.Status == SubOrderStatus.Preparing
                       || s.Status == SubOrderStatus.ReadyForPickup);

            bool orderGroupAdvanced = false;

            if (allAccepted && og.Status == OrderGroupStatus.AwaitingMerchantAcceptance)
            {
                og.Status = OrderGroupStatus.AwaitingDriverAssignment;
                orderGroupAdvanced = true;
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 6: AuditLog — registro inmutable de la aceptación.
            // ─────────────────────────────────────────────────────────────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(SubOrder),
                EntityId = subOrder.Id,
                Action = "AcceptSubOrder",
                PerformedByUserId = command.MerchantId,
                //PerformedByRole = "Merchant",
                Details = $"SubOrder {subOrder.Id} aceptada por Merchant {command.MerchantId}." +
                                          (orderGroupAdvanced ? " OrderGroup avanzó a AwaitingDriverAssignment." : ""),
                RequiresImmediateAttention = false,
                CreatedAt = DateTime.UtcNow
            });

            // ─────────────────────────────────────────────────────────────
            // PASO 7: Persistir todo en una sola transacción.
            // ─────────────────────────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // ─────────────────────────────────────────────────────────────
            // PASO 8: Si el grupo avanzó, disparar StartDispatch.
            //
            // Se ejecuta DESPUÉS del SaveChanges para garantizar que el
            // estado del OrderGroup ya está persistido antes de que el
            // StartDispatchCommandHandler lo lea.
            // ─────────────────────────────────────────────────────────────
            if (orderGroupAdvanced)
            {
                await _bus.SendAsync<StartDispatchCommand, StartDispatchResult>(
                    new StartDispatchCommand(og.Id), ct);
            }

            return new AcceptSubOrderResult
            {
                Success = true,
                Message = orderGroupAdvanced
                    ? "SubOrder aceptada. Todos los comercios confirmaron. Iniciando búsqueda de repartidor."
                    : "SubOrder aceptada. Esperando confirmación de otros comercios del pedido.",
                AllSubOrdersAccepted = orderGroupAdvanced,
                NewOrderGroupStatus = orderGroupAdvanced
                    ? nameof(OrderGroupStatus.AwaitingDriverAssignment)
                    : null
            };
        }

        private static AcceptSubOrderResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
