// PROPÓSITO: Handler de solo lectura para GetMerchantOrdersQuery.
//            Proyecta SubOrders del comercio + su OrderGroup padre a DTO.
//            AsNoTracking — solo lectura, máximo performance.
//

using Manda2.Application.Common;
using Manda2.Application.Feature.Merchant.Dtos;
using Manda2.Application.Feature.OrderGroups.Queries;
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
    /// <summary>
    /// Handler para <see cref="GetMerchantOrdersQuery"/>.
    /// </summary>
    public class GetMerchantOrdersHandler
        : IQueryHandler<GetMerchantOrdersQuery, PagedResult<MerchantOrderGroupDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetMerchantOrdersHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<MerchantOrderGroupDto>> HandleAsync(
            GetMerchantOrdersQuery query, CancellationToken ct)
        {
            // ── PASO 1: Base query — SubOrders del comercio ────
            var baseQuery = _db.SubOrders
                .AsNoTracking()
                .Where(s => s.MerchantId == query.MerchantId && !s.IsDeleted);

            // ── PASO 2: Filtro opcional por estado ────
            if (!string.IsNullOrWhiteSpace(query.SubOrderStatusFilter)
                && Enum.TryParse<SubOrderStatus>(query.SubOrderStatusFilter,
                    ignoreCase: true, out var parsedStatus))
            {
                baseQuery = baseQuery.Where(s => s.Status == parsedStatus);
            }

            // ── PASO 3: Contar total para paginación ────
            var pageSize = query.PageSize > 50 ? 50 : query.PageSize < 1 ? 20 : query.PageSize;
            var page = query.Page < 1 ? 1 : query.Page;
            var total = await baseQuery.CountAsync(ct);

            // ── PASO 4: Proyectar a DTO con paginación ────
            var items = await baseQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new MerchantOrderGroupDto
                {
                    OrderGroupId = s.OrderGroup.Id,
                    OrderGroupStatus = s.OrderGroup.Status.ToString(),
                    DeliveryAddressText = s.OrderGroup.DeliveryAddressText,
                    PaymentMethodName = s.OrderGroup.PaymentMethodName,
                    TotalAmount = s.OrderGroup.TotalAmount,
                    CreatedAt = s.OrderGroup.CreatedAt,
                    DeliveredAt = s.OrderGroup.DeliveredAt,

                    SubOrder = new MerchantSubOrderSummaryDto
                    {
                        SubOrderId = s.Id,
                        Status = s.Status.ToString(),
                        SubTotal = s.SubTotal,
                        NetPayable = s.NetPayable,
                        AcceptedAt = s.AcceptedAt,
                        ReadyAt = s.ReadyAt,
                        PickedUpAt = s.PickedUpAt,
                        DeliveredAt = s.DeliveredAt,
                        CancelledAt = s.CancelledAt
                    }
                })
                .ToListAsync(ct);

            // ── PASO 5: Ensamblar resultado paginado ────
            return new PagedResult<MerchantOrderGroupDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
