using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un pago parcial o total para un OrderGroup.
    /// Permite el manejo de múltiples medios de pago (Split Payment).
    /// </summary>
    public class OrderGroupPayment : BaseEntity
    {
        /// <summary>
        /// Referencia al grupo de órdenes al que pertenece este pago.
        /// </summary>
        public int OrderGroupId { get; set; }
        public virtual OrderGroup OrderGroup { get; set; } = null!;

        /// <summary>
        /// Referencia al medio de pago utilizado (Tarjeta, Efectivo, Transferencia).
        /// </summary>
        public int PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; } = null!;

        /// <summary>
        /// Monto pagado en este registro específico.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Estado del pago (Pending, Confirmed, etc.)
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Referencia externa provista por el banco, cliente o pasarela.
        /// En Transferencias, es el número de referencia del depósito.
        /// </summary>
        public string? Reference { get; set; }

        /// <summary>
        /// Número de autorización devuelto por el procesador de pagos.
        /// Este campo es llenado por el BackOffice en transferencias manuales.
        /// </summary>
        public string? Authorization { get; set; }

        /// <summary>
        /// URL o ruta del comprobante adjunto (imagen del recibo) para validación manual.
        /// </summary>
        public string? ProofUrl { get; set; }

        /// <summary>
        /// ID del usuario de BackOffice que realizó la validación manual (si aplica).
        /// </summary>
        public int? ConfirmedByUserId { get; set; }

        /// <summary>
        /// Fecha y hora en que se confirmó el pago.
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }
    }
}
