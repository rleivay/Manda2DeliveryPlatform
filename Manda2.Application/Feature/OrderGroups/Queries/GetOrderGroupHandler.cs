using Manda2.Application.Common;
using Manda2.Application.Feature.OrderGroups.Dtos;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Queries
{
    /// <summary>
    /// Handler para <see cref="GetOrderGroupQuery"/>.
    /// </summary>
    public class GetOrderGroupHandler : IQueryHandler<GetOrderGroupQuery, OrderGroupDetailDto>
    {
        private readonly IApplicationDbContext _db;

        public GetOrderGroupHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<OrderGroupDetailDto> HandleAsync(
            GetOrderGroupQuery query, CancellationToken ct)
        {
            var result = await _db.OrderGroups
                .AsNoTracking()
                .Where(og => og.Id == query.OrderGroupId)
                .Select(og => new OrderGroupDetailDto
                {
                    Id = og.Id,
                    Status = og.Status.ToString(),
                    CustomerId = og.CustomerId,
                    DriverId = og.DriverId,

                    // Driver: FirstName + LastName (no existe FullName)
                    DriverName = og.Driver != null
                        ? og.Driver.FirstName + " " + og.Driver.LastName
                        : "Asignando...",

                    // Driver: PhoneNumber (no existe Phone)
                    DriverPhone = og.Driver != null ? og.Driver.PhoneNumber : null,

                    // Financiero — campos reales de OrderGroup
                    TotalAmount = og.TotalAmount,
                    DeliveryFee = og.DeliveryFee,
                    PaymentMethodName = og.PaymentMethodName,

                    // Dirección — campo real: DeliveryAddressText
                    DeliveryAddressText = og.DeliveryAddressText,

                    // SubOrders — campo real de monto: SubTotal (no TotalAmount)
                    SubOrders = og.SubOrders.Select(so => new SubOrderDetailDto
                    {
                        SubOrderId = so.Id,
                        MerchantName = so.Merchant.Name,
                        Status = so.Status.ToString(),
                        SubTotal = so.SubTotal
                    }).ToList(),

                    // Stops — campo real de dirección: AddressText (no AddressLabel)
                    Stops = og.Stops
                        .OrderBy(s => s.Sequence)
                        .Select(s => new OrderStopDto
                        {
                            StopId = s.Id,
                            Sequence = s.Sequence,
                            Type = s.StopType.ToString(),
                            AddressText = s.AddressText,
                            Latitude = s.Latitude,
                            Longitude = s.Longitude,
                            IsCompleted = s.CompletedAt != null,
                            CompletedAt = s.CompletedAt,
                            Notes = s.Notes
                        }).ToList(),

                    CreatedAt = og.CreatedAt,
                    DeliveredAt = og.DeliveredAt
                })
                .FirstOrDefaultAsync(ct);

            if (result == null)
                throw new KeyNotFoundException(
                    $"OrderGroup {query.OrderGroupId} no encontrado.");

            return result;
        }
    }
}
