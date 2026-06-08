// PROPÓSITO: Handler de solo lectura para GetDriverActiveGroupQuery.
//            Navega desde Driver.CurrentOrderGroupId hacia el OrderGroup
//            y proyecta directamente a OrderGroupDetailDto (AsNoTracking).
//
// CAMPOS VERIFICADOS:
//   Driver          → Id, CurrentOrderGroupId, FirstName, LastName, PhoneNumber
//   OrderGroup      → Id, Status, CustomerId, DriverId, TotalAmount, DeliveryFee,
//                     PaymentMethodName, DeliveryAddressText, CreatedAt, DeliveredAt
//   SubOrder        → Id, Status, SubTotal, Merchant.Name
//   OrderGroupStop  → Id, Sequence, StopType, AddressText, Latitude, Longitude,
//                     CompletedAt, Notes
// ════════════════════════════════════════════════════════════════════════════

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
    /// Handler para <see cref="GetDriverActiveGroupQuery"/>.
    /// </summary>
    public class GetDriverActiveGroupHandler : IQueryHandler<GetDriverActiveGroupQuery, OrderGroupDetailDto?>
    {
        private readonly IApplicationDbContext _db;

        public GetDriverActiveGroupHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<OrderGroupDetailDto?> HandleAsync(
            GetDriverActiveGroupQuery query, CancellationToken ct)
        {
            // Paso 1: Obtener CurrentOrderGroupId del driver
            var driver = await _db.Drivers
                .AsNoTracking()
                .Where(d => d.Id == query.DriverId)
                .Select(d => new { d.CurrentOrderGroupId, d.FirstName, d.LastName, d.PhoneNumber })
                .FirstOrDefaultAsync(ct);

            if (driver == null)
                throw new KeyNotFoundException($"Driver {query.DriverId} no encontrado.");

            // Si no tiene grupo activo, retornamos null (la app muestra "Sin pedido activo")
            if (driver.CurrentOrderGroupId == null)
                return null;

            // Paso 2: Proyectar el OrderGroup activo a DTO
            var result = await _db.OrderGroups
                .AsNoTracking()
                .Where(og => og.Id == driver.CurrentOrderGroupId.Value)
                .Select(og => new OrderGroupDetailDto
                {
                    Id = og.Id,
                    Status = og.Status.ToString(),
                    CustomerId = og.CustomerId,
                    DriverId = og.DriverId,

                    // Nombre del driver desde los campos reales
                    DriverName = driver.FirstName + " " + driver.LastName,
                    DriverPhone = driver.PhoneNumber,

                    // Financiero
                    TotalAmount = og.TotalAmount,
                    DeliveryFee = og.DeliveryFee,
                    PaymentMethodName = og.PaymentMethodName,

                    // Dirección
                    DeliveryAddressText = og.DeliveryAddressText,

                    // SubOrders
                    SubOrders = og.SubOrders.Select(so => new SubOrderDetailDto
                    {
                        SubOrderId = so.Id,
                        MerchantName = so.Merchant.Name,
                        Status = so.Status.ToString(),
                        SubTotal = so.SubTotal
                    }).ToList(),

                    // Stops ordenados por secuencia
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

            return result;
        }
    }
}
