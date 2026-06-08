using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Constants;
using Manda2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Commands
{
    public class CreateShippingAddressCommandHandler
         : ICommandHandler<CreateShippingAddressCommand, int>
    {
        private readonly IApplicationDbContext _db;

        public CreateShippingAddressCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> HandleAsync(
            CreateShippingAddressCommand command,
            CancellationToken ct)
        {
            var address = new ShippingAddress
            {
                CustomerId = command.CustomerId,
                Alias = command.Alias.Trim(),
                AddressLine1 = command.AddressLine1.Trim(),
                AddressLine2 = string.IsNullOrWhiteSpace(command.AddressLine2)
        ? null
        : command.AddressLine2.Trim(),
                City = command.City.Trim(),
                Latitude = command.Latitude,
                Longitude = command.Longitude,
                IsDefault = command.IsDefault,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = command.CreatedByUserId,
                ContactPhone = string.IsNullOrWhiteSpace(command.ContactPhone)
        ? null
        : command.ContactPhone.Trim(),
                PhoneCountryCode = string.IsNullOrWhiteSpace(command.PhoneCountryCode)
        ? null
        : command.PhoneCountryCode.Trim(),
                ApartmentNumber = string.IsNullOrWhiteSpace(command.ApartmentNumber)
        ? null
        : command.ApartmentNumber.Trim(),
                DeliveryNotes = string.IsNullOrWhiteSpace(command.DeliveryNotes)
        ? null
        : command.DeliveryNotes.Trim()
            };

            if (command.IsDefault)
            {
                var currentDefaults = await _db.ShippingAddresses
                    .Where(x =>
                        x.CustomerId == command.CustomerId &&
                        x.IsDefault &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .ToListAsync(ct);

                foreach (var item in currentDefaults)
                {
                    item.IsDefault = false;
                    item.UpdatedAt = DateTime.UtcNow;
                    item.UpdatedByUserId = command.CreatedByUserId;
                }
            }

            _db.ShippingAddresses.Add(address);
            await _db.SaveChangesAsync(ct);

            return address.Id;
        }
    }
}
