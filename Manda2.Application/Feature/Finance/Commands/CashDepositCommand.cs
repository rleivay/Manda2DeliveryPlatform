// PROPÓSITO: Contrato de entrada para registrar un depósito de efectivo
//            del driver al operador.
//
// FLUJO:
//   Driver entrega efectivo acumulado → Operador registra el depósito →
//   Sistema reduce CurrentCashBalance del driver →
//   Si balance < MaxCashLimit → desbloquea al driver (IsCashBlocked = false).
//

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Finance.Commands
{
    /// <summary>
    /// Comando para registrar un depósito de efectivo del driver al operador.
    /// Solo BackOffice/Admin puede ejecutar este comando.
    /// </summary>
    public class CashDepositCommand : ICommand<CashDepositResult>
    {
        /// <summary>ID del driver que realiza el depósito.</summary>
        public int DriverId { get; set; }

        /// <summary>Monto depositado en moneda local (GTQ).</summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Ruta o URL del comprobante físico escaneado (opcional en MVP).
        /// Ej: "/uploads/vouchers/driver5_20260520.jpg"
        /// </summary>
        public string? VoucherPath { get; set; }

        /// <summary>
        /// Notas del operador que recibe el depósito.
        /// Ej: "Depósito en efectivo recibido en oficina central."
        /// </summary>
        public string? ReviewNotes { get; set; }

        /// <summary>
        /// ID del usuario de BackOffice que registra el depósito.
        /// Se guarda en ReviewedBy del CashDeposit y en AuditLog.
        /// </summary>
        public int ReceivedByUserId { get; set; }
    }
}
