using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Customer
{
    public class CreateShippingAddressRequest
    {
        public string Alias { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsDefault { get; set; }

        // 🆕 Campos para el formulario móvil
        public string? ApartmentNumber { get; set; }
        public string? DeliveryNotes { get; set; }
        public string? PhoneCountryCode { get; set; }
        public string? ContactPhone { get; set; }

        public string FullDisplayName => $"{Alias}: {AddressLine1} {(string.IsNullOrEmpty(ApartmentNumber) ? "" : "#" + ApartmentNumber)}";
    }
}
