using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Customer.Commands
{
    public class CreateShippingAddressCommand : ICommand<int>
    {
        public int CustomerId { get; set; }
        public int? CreatedByUserId { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsDefault { get; set; }
        public string? ApartmentNumber { get; init; }
        public string? DeliveryNotes { get; init; }
        public string? PhoneCountryCode { get; init; }
        public string? ContactPhone { get; init; }
    }
}
