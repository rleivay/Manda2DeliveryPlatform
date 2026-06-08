using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class CustomerCreditNote : BaseEntity
    {
        public int CustomerId { get; set; }
        public int CreditNoteTypeId { get; set; }
        public CreditNoteType CreditNoteType { get; set; } = null!;

        // Campos SAP
        public int? SAP_DocEntry { get; set; }
        public string? SAP_DocNum { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; } // Saldo disponible
        public bool IsActive { get; set; } = true;
    }
}
