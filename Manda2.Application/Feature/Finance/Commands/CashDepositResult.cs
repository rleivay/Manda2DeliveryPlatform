using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Finance.Commands
{
    /// <summary>
    /// Resultado del comando CashDepositCommand.
    /// </summary>
    public class CashDepositResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>ID del registro CashDeposit creado.</summary>
        public int? CashDepositId { get; set; }

        /// <summary>Balance de efectivo del driver después del depósito.</summary>
        public decimal NewCashBalance { get; set; }

        /// <summary>True si el driver fue desbloqueado como resultado del depósito.</summary>
        public bool DriverUnblocked { get; set; }
    }
}
