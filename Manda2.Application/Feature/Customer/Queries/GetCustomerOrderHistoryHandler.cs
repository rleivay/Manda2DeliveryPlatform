// PROPÓSITO: Handler de solo lectura para GetCustomerOrderHistoryQuery.
//            Proyecta OrderGroups del cliente + nombres de comercios a DTO.
//            AsNoTracking — solo lectura, máximo performance.
//

using Manda2.Application.Common;
using Manda2.Application.Feature.Customer.Dtos;
using Manda2.Application.Feature.OrderGroups.Queries;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Queries
{
    /// <summary>
    /// Handler para <see cref="GetCustomerOrderHistoryQuery"/>.
    /// </summary>
    public class GetCustomerOrderHistoryHandler
        : IQueryHandler<GetCustomerOrderHistoryQuery, PagedResult<CustomerOrderHistoryDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetCustomerOrderHistoryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<CustomerOrderHistoryDto>> HandleAsync(
            GetCustomerOrderHistoryQuery query, CancellationToken ct)
        {
            // ── PASO 1: Base query — OrderGroups del cliente ────
            var baseQuery = _db.OrderGroups
                .AsNoTracking()
                .Where(og => og.CustomerId == query.CustomerId && !og.IsDeleted);

            // ── PASO 2: Contar total para paginación ────
            var total = await baseQuery.CountAsync(ct);

            // ── PASO 3: Proyectar a DTO con paginación ────
            // Los nombres de comercios se obtienen desde SubOrders → Merchant.Name
            var items = await baseQuery
                .OrderByDescending(og => og.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(og => new CustomerOrderHistoryDto
                {
                    OrderGroupId = og.Id,
                    Status = og.Status.ToString(),
                    TotalAmount = og.TotalAmount,
                    DeliveryFee = og.DeliveryFee,
                    ServiceFee = og.ServiceFee,
                    DeliveryAddressText = og.DeliveryAddressText,
                    PaymentMethodName = og.PaymentMethodName,
                    CreatedAt = og.CreatedAt,
                    DeliveredAt = og.DeliveredAt,

                    // Proyectar nombres de comercios desde SubOrders
                    MerchantNames = og.SubOrders
                        .Where(s => !s.IsDeleted)
                        .Select(s => s.Merchant.Name)
                        .ToList()
                })
                .ToListAsync(ct);

            // ── PASO 4: Ensamblar resultado paginado ────
            return new PagedResult<CustomerOrderHistoryDto>
            {
                Items = items,
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
