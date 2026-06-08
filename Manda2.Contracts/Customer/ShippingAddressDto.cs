using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Customer
{
    public class ShippingAddressDto
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string Alias { get; set; } = string.Empty;

        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        public string City { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsDefault { get; set; }

        public string? ContactPhone { get; set; }

        public string? PhoneCountryCode { get; set; }

        public string? ApartmentNumber { get; set; }

        public string? DeliveryNotes { get; set; }
    }
}
