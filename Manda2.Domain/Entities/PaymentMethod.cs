using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class PaymentMethod : BaseEntity
    {
        public string Name { get; set; } = null!; // Efectivo, Tarjeta, NC, etc.
        public string Code { get; set; } = null!; // CASH, CARD, TRANSFER, CREDIT_NOTE
        public bool RequiresVoucher { get; set; }
        public bool RequiresReference { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
