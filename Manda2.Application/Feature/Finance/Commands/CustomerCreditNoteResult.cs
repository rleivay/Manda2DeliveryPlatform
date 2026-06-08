using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Finance.Commands
{
    /// <summary>
    /// Resultado del comando CustomerCreditNoteCommand.
    /// </summary>
    public class CustomerCreditNoteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>ID del registro CustomerCreditNote creado.</summary>
        public int? CreditNoteId { get; set; }

        /// <summary>Monto total de la nota emitida.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Nombre del tipo de nota de crédito aplicado.</summary>
        public string CreditNoteTypeName { get; set; } = string.Empty;
    }
}
