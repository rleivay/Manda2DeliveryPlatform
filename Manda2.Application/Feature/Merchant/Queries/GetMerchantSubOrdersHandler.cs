using Manda2.Application.Common;
using Manda2.Application.Feature.Merchant.Dtos;
using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Queries
{
    public class GetMerchantSubOrdersHandler
        : IQueryHandler<GetMerchantSubOrdersQuery, List<MerchantSubOrderDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetMerchantSubOrdersHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<MerchantSubOrderDto>> HandleAsync(
            GetMerchantSubOrdersQuery query, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // Estados activos de la cola de trabajo del comercio.
            // No mostramos Delivered, Cancelled, RejectedByMerchant
            // en la vista operativa (son históricos).
            // ─────────────────────────────────────────────────────────────
            var activeStatuses = new[]
            {
                SubOrderStatus.PendingMerchantAcceptance,
                SubOrderStatus.AcceptedByMerchant,
                SubOrderStatus.Preparing,
                SubOrderStatus.ReadyForPickup
            };

            var q = _db.SubOrders
                .AsNoTracking()
                .Include(s => s.Details)
                .Where(s => s.MerchantId == query.MerchantId
                         && activeStatuses.Contains(s.Status));

            // Filtro opcional por estado
            if (!string.IsNullOrWhiteSpace(query.StatusFilter)
                && Enum.TryParse<SubOrderStatus>(query.StatusFilter, out var parsedStatus))
            {
                q = q.Where(s => s.Status == parsedStatus);
            }

            return await q
                .OrderBy(s => s.CreatedAt) // FIFO: primero los más antiguos
                .Select(s => new MerchantSubOrderDto
                {
                    SubOrderId = s.Id,
                    OrderGroupId = s.OrderGroupId,
                    Status = s.Status.ToString(),
                    SubTotal = s.SubTotal,
                    NetPayable = s.NetPayable,
                    CreatedAt = s.CreatedAt,
                    AcceptedAt = s.AcceptedAt,
                    ReadyAt = s.ReadyAt,
                    Details = s.Details.Select(d => new MerchantSubOrderDetailDto
                    {
                        DetailId = d.Id,
                        ItemName = d.ItemName,
                        Quantity = d.Quantity,
                        SalePrice = d.SalePrice,
                        LineTotal = d.LineTotal,
                        Notes = d.Notes
                    }).ToList()
                })
                .ToListAsync(ct);
        }
    }
}
