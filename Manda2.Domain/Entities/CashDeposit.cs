using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class CashDeposit : BaseEntity
    {
        public int DriverId { get; set; }
        public decimal Amount { get; set; }
        public string? VoucherPath { get; set; } // Ruta física o URL del voucher
        public DateTime DepositDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // AUDITORÍA
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }

        // RELACIÓN
        public Driver Driver { get; set; } = null!;
    }
}
