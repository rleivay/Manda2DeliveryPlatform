using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class ShippingAddress : BaseEntity
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Alias { get; set; } = null!; // Casa, Oficina, etc.
        
        [Required]
        [MaxLength(200)]
        public string AddressLine1 { get; set; } = null!;

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Column(TypeName = "decimal(18, 10)")]
        public double Latitude { get; set; }

        [Column(TypeName = "decimal(18, 10)")]
        public double Longitude { get; set; }


        public bool IsDefault { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(5)]
        public string? PhoneCountryCode { get; set; }

        [MaxLength(50)]
        public string? ApartmentNumber { get; set; }

        [MaxLength(300)]
        public string? DeliveryNotes { get; set; }

        // Relación
        public virtual Customer Customer { get; set; } = null!;
    }
}
