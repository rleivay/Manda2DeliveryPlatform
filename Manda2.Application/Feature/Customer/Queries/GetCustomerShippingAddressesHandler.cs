using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Contracts.Customer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Queries
{
    /// <summary>
    /// Handler de solo lectura. AsNoTracking — máximo performance.
    /// </summary>
    public class GetCustomerShippingAddressesHandler
        : IQueryHandler<GetCustomerShippingAddressesQuery, List<ShippingAddressDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetCustomerShippingAddressesHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ShippingAddressDto>> HandleAsync(
            GetCustomerShippingAddressesQuery query,
            CancellationToken ct)
        {
            return await _db.ShippingAddresses
                .AsNoTracking()
                .Where(sa => sa.CustomerId == query.CustomerId
                          && sa.IsActive
                          && !sa.IsDeleted)
                .OrderByDescending(sa => sa.IsDefault)
                .ThenBy(sa => sa.Alias)
                .Select(x => new ShippingAddressDto
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    Alias = x.Alias,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    City = x.City,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    IsDefault = x.IsDefault,
                    ContactPhone = x.ContactPhone,
                    PhoneCountryCode = x.PhoneCountryCode,
                    ApartmentNumber = x.ApartmentNumber,
                    DeliveryNotes = x.DeliveryNotes
                })
                .ToListAsync(ct);
        }
    }
}
