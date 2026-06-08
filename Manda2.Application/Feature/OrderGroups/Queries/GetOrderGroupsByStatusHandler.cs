// PROPÓSITO: Handler de solo lectura para GetOrderGroupsByStatusQuery.
//            Proyecta OrderGroup + Customer + Driver a OrderGroupSummaryDto.
//            AsNoTracking — solo lectura, máximo performance.
//

using Manda2.Application.Common;
using Manda2.Application.Feature.OrderGroups.Dtos;
using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Queries
{
    /// <summary>
    /// Handler para <see cref="GetOrderGroupsByStatusQuery"/>.
    /// </summary>
    public class GetOrderGroupsByStatusHandler
        : IQueryHandler<GetOrderGroupsByStatusQuery, PagedResult<OrderGroupSummaryDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetOrderGroupsByStatusHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<OrderGroupSummaryDto>> HandleAsync(
            GetOrderGroupsByStatusQuery query, CancellationToken ct)
        {
            // ── PASO 1: Parsear el Status string a enum ────
            if (!Enum.TryParse<OrderGroupStatus>(query.Status, ignoreCase: true, out var statusEnum))
                throw new ArgumentException(
                    $"Status '{query.Status}' no es válido. " +
                    $"Valores permitidos: {string.Join(", ", Enum.GetNames<OrderGroupStatus>())}");

            // ── PASO 2: Base query con filtro de status ────
            var baseQuery = _db.OrderGroups
                .AsNoTracking()
                .Where(og => og.Status == statusEnum && !og.IsDeleted);

            // ── PASO 3: Contar total para paginación ────
            var totalCount = await baseQuery.CountAsync(ct);

            // ── PASO 4: Proyectar a DTO con paginación ────
            var items = await baseQuery
                .OrderByDescending(og => og.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(og => new OrderGroupSummaryDto
                {
                    Id = og.Id,
                    Status = og.Status.ToString(),
                    CustomerId = og.CustomerId,

                    // Nombre del cliente desde navegación
                    CustomerName = og.Customer.FirstName + " " + og.Customer.LastName,

                    DriverId = og.DriverId,

                    // Nombre del driver: null si no asignado
                    DriverName = og.Driver != null
                        ? og.Driver.FirstName + " " + og.Driver.LastName
                        : null,

                    SubOrderCount = og.SubOrderCount,
                    TotalAmount = og.TotalAmount,
                    DeliveryFee = og.DeliveryFee,
                    ServiceFee = og.ServiceFee,
                    PaymentMethodName = og.PaymentMethodName,
                    DeliveryAddressText = og.DeliveryAddressText,
                    CreatedAt = og.CreatedAt,
                    DeliveredAt = og.DeliveredAt
                })
                .ToListAsync(ct);

            // ── PASO 5: Ensamblar resultado paginado ────
            return new PagedResult<OrderGroupSummaryDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
