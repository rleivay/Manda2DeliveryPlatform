/// Define el estado actual de un registro de pago individual.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Enum
{
    public enum PaymentStatus
    {
        /// <summary>
        /// Pago registrado pero aún no confirmado (ej. Transferencia pendiente de validar).
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Pago validado y confirmado (ej. Pasarela OK o BackOffice validó transferencia).
        /// </summary>
        Confirmed = 2,

        /// <summary>
        /// Pago rechazado por la pasarela o BackOffice.
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Pago anulado manualmente por el administrador o por cancelación de orden.
        /// </summary>
        Voided = 4
    }
}
