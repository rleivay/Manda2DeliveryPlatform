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
    public class UpdateCartItemQuantityCommandHandler
        : ICommandHandler<UpdateCartItemQuantityCommand, UpdateCartItemQuantityResult>
    {
        private readonly IApplicationDbContext _db;

        public UpdateCartItemQuantityCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UpdateCartItemQuantityResult> HandleAsync(
            UpdateCartItemQuantityCommand command, CancellationToken ct = default)
        {
            // ── 1. Validar cantidad mínima ────
            if (command.NewQuantity < 1)
                throw new BusinessRuleException("La cantidad mínima es 1. Para eliminar usa el endpoint de eliminación.");

            // ── 2. Cargar detail con jerarquía completa ────
            var detail = await _db.SubOrderDetails
                .Include(d => d.SubOrder)
                    .ThenInclude(s => s.OrderGroup)
                .FirstOrDefaultAsync(d => d.Id == command.SubOrderDetailId, ct);

            if (detail == null)
                throw new BusinessRuleException("Item no encontrado en el carrito.");

            // ── 3. Validar ownership ────
            if (detail.SubOrder.OrderGroup.CustomerId != command.CustomerId)
                throw new BusinessRuleException("No tienes acceso a este item.");

            // ── 4. Validar estado Draft ────
            if (detail.SubOrder.OrderGroup.Status != Manda2.Contracts.Enum.OrderGroupStatus.Draft)
                throw new BusinessRuleException("Solo se puede editar un carrito en estado Draft.");

            // ── 5. Recalcular línea ────
            detail.Quantity = command.NewQuantity;
            detail.LineTotal = detail.NetSalePrice * command.NewQuantity;
            detail.TaxAmount = detail.LineTotal * (detail.TaxPct / 100);
            detail.LineTotalWithTax = detail.LineTotal + detail.TaxAmount;

            // ── 6. Recalcular SubOrder.SubTotal ────
            var subOrder = detail.SubOrder;
            var allDetails = await _db.SubOrderDetails
                .Where(d => d.SubOrderId == subOrder.Id)
                .ToListAsync(ct);

            // Aplicar el cambio en memoria antes de sumar
            var updatedDetail = allDetails.First(d => d.Id == detail.Id);
            updatedDetail.Quantity = detail.Quantity;
            updatedDetail.LineTotal = detail.LineTotal;
            updatedDetail.TaxAmount = detail.TaxAmount;
            updatedDetail.LineTotalWithTax = detail.LineTotalWithTax;

            subOrder.SubTotal = allDetails.Sum(d => d.LineTotal);
            subOrder.TotalCommissionAmount = subOrder.SubTotal * (subOrder.ResolvedCommissionPct / 100);
            subOrder.NetPayable = subOrder.SubTotal
                - subOrder.TotalCommissionAmount
                - subOrder.DeliveryFeeProrrated;

            // ── 7. Recalcular OrderGroup.TotalAmount ────
            var group = subOrder.OrderGroup;
            var allSubOrders = await _db.SubOrders
                .Where(s => s.OrderGroupId == group.Id)
                .ToListAsync(ct);

            // Aplicar cambio en memoria
            var updatedSubOrder = allSubOrders.First(s => s.Id == subOrder.Id);
            updatedSubOrder.SubTotal = subOrder.SubTotal;

            group.TotalAmount = allSubOrders.Sum(s => s.SubTotal)
                + group.DeliveryFee
                + group.ServiceFee;

            await _db.SaveChangesAsync(ct);

            return new UpdateCartItemQuantityResult(
                Success: true,
                Message: "Cantidad actualizada correctamente.",
                NewLineTotal: detail.LineTotal
            );
        }
    }
}
