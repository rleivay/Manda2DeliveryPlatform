using Manda2.Application.Common;
using Manda2.Application.Common.Exceptions;
using Manda2.Application.Mediator;
using Manda2.Contracts.Cart;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Cart.Queries
{
    /// <summary>
    /// Handler de GetCartQuery.
    /// Construye el CartDto desde OrderGroup → SubOrders → SubOrderDetails.
    /// </summary>
    public class GetCartQueryHandler : IQueryHandler<GetCartQuery, CartDto>
    {
        private readonly IApplicationDbContext _db;

        public GetCartQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CartDto> HandleAsync(GetCartQuery query, CancellationToken ct = default)
        {
            // ── 1. Cargar OrderGroup con toda la jerarquía ────
            var group = await _db.OrderGroups
                .AsNoTracking()
                .Include(g => g.SubOrders)
                    .ThenInclude(s => s.Merchant)
                .Include(g => g.SubOrders)
                    .ThenInclude(s => s.Details)
                .FirstOrDefaultAsync(g => g.Id == query.OrderGroupId, ct);

            if (group == null)
                throw new BusinessRuleException("El carrito no existe.");

            // ── 2. Validar ownership ────
            if (group.CustomerId != query.CustomerId)
                throw new BusinessRuleException("No tienes acceso a este carrito.");

            // ── 3. Validar estado (solo Draft es editable) ────
            if (group.Status != OrderGroupStatus.Draft)
                throw new BusinessRuleException(
                    $"El carrito no está en estado Draft. Estado actual: {group.Status}.");

            // ── 4. Construir DTO ────
            var merchants = group.SubOrders
                .Where(s => s.Details.Any())
                .Select(s => new CartMerchantDto
                {
                    MerchantId = s.MerchantId,
                    MerchantName = s.Merchant.Name,
                    Subtotal = s.SubTotal,
                    Items = s.Details.Select(d => new CartItemDto
                    {
                        SubOrderDetailId = d.Id,
                        ItemName = d.ItemName,
                        SnapshotImageUrl = d.SnapshotImageUrl,
                        Quantity = d.Quantity,
                        UnitPrice = d.NetSalePrice,
                        LineTotal = d.LineTotal,
                        LineTotalWithTax = d.LineTotalWithTax,
                        Notes = d.Notes
                    }).ToList()
                }).ToList();

            return new CartDto
            {
                OrderGroupId = group.Id,
                Status = group.Status.ToString(),
                DeliveryAddressText = group.DeliveryAddressText,
                Subtotal = group.SubOrders.Sum(s => s.SubTotal),
                DeliveryFee = group.DeliveryFee,
                ServiceFee = group.ServiceFee,
                Total = group.TotalAmount,
                Merchants = merchants
            };
        }
    }
}
