using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class Customer : BaseEntity
    {
        
        // Datos básicos
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        // Adjuntos / identidad para KYC
        public string? IdentityDocumentUrl { get; set; } // URL o ruta del archivo subido

        // Flags de activación (BackOffice valida el adjunto)
        public bool IsActive { get; set; } = false;

        // Relaciones (si aplica)
        // public ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
    }
}
