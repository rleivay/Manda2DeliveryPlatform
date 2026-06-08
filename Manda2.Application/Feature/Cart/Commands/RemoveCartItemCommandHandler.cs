using Manda2.Application.Common;
using Manda2.Application.Common.Exceptions;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Cart.Commands
{
    public class RemoveCartItemCommandHandler
        : ICommandHandler<RemoveCartItemCommand, RemoveCartItemResult>
    {
        private readonly IApplicationDbContext _db;

        public RemoveCartItemCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RemoveCartItemResult> HandleAsync(
            RemoveCartItemCommand command, CancellationToken ct = default)
        {
            // ── 1. Cargar detail con jerarquía ────
            var detail = await _db.SubOrderDetails
                .Include(d => d.SubOrder)
                    .ThenInclude(s => s.OrderGroup)
                .FirstOrDefaultAsync(d => d.Id == command.SubOrderDetailId, ct);

            if (detail == null)
                throw new BusinessRuleException("Item no encontrado.");

            // ── 2. Validar ownership ────
            if (detail.SubOrder.OrderGroup.CustomerId != command.CustomerId)
                throw new BusinessRuleException("No tienes acceso a este item.");

            // ── 3. Validar Draft ────
            if (detail.SubOrder.OrderGroup.Status != Manda2.Contracts.Enum.OrderGroupStatus.Draft)
                throw new BusinessRuleException("Solo se puede editar un carrito en estado Draft.");

            var subOrder = detail.SubOrder;
            var group = subOrder.OrderGroup;

            // ── 4. Eliminar el detail ────
            _db.SubOrderDetails.Remove(detail);
            await _db.SaveChangesAsync(ct);

            // ── 5. ¿SubOrder quedó vacía? ────
            bool subOrderRemoved = false;
            bool orderGroupRemoved = false;

            int remainingDetails = await _db.SubOrderDetails
                .CountAsync(d => d.SubOrderId == subOrder.Id, ct);

            if (remainingDetails == 0)
            {
                _db.SubOrders.Remove(subOrder);
                await _db.SaveChangesAsync(ct);
                subOrderRemoved = true;

                // ── 6. ¿OrderGroup quedó vacío? ────
                int remainingSubOrders = await _db.SubOrders
                    .CountAsync(s => s.OrderGroupId == group.Id, ct);

                if (remainingSubOrders == 0)
                {
                    _db.OrderGroups.Remove(group);
                    await _db.SaveChangesAsync(ct);
                    orderGroupRemoved = true;
                }
            }

            // ── 7. Recalcular totales si el grupo sigue vivo ────
            if (!orderGroupRemoved)
            {
                var allSubOrders = await _db.SubOrders
                    .Where(s => s.OrderGroupId == group.Id)
                    .ToListAsync(ct);

                group.SubOrderCount = allSubOrders.Count;
                group.TotalAmount = allSubOrders.Sum(s => s.SubTotal)
                    + group.DeliveryFee
                    + group.ServiceFee;

                await _db.SaveChangesAsync(ct);
            }

            return new RemoveCartItemResult(
                Success: true,
                Message: orderGroupRemoved
                    ? "Carrito eliminado (sin items)."
                    : subOrderRemoved
                        ? "Comercio eliminado del carrito."
                        : "Item eliminado correctamente.",
                SubOrderRemoved: subOrderRemoved,
                OrderGroupRemoved: orderGroupRemoved
            );
        }
    }
}
