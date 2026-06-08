using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class CreditNoteType : BaseEntity
    {
        public string Name { get; set; } = null!; // Nota de Crédito, GiftCard, Devolución
        public bool IsActive { get; set; } = true;
    }
}
