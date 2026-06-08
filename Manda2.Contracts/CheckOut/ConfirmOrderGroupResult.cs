using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.CheckOut
{
    /// <summary>
    /// Resultado de la consolidación logística del checkout.
    /// El Mobile usa estos valores para mostrar el resumen antes del pago.
    /// </summary>
    public class ConfirmOrderGroupResult
    {
        /// <summary>ID del OrderGroup consolidado.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>Estado resultante. Siempre "CapacityValidated" en éxito.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Subtotal de productos (sin fees).</summary>
        public decimal SubTotal { get; set; }

        /// <summary>Fee de delivery calculado con Haversine real.</summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>Fee de servicio fijo (SERVICE_FEE_FIXED).</summary>
        public decimal ServiceFee { get; set; }

        /// <summary>Total final = SubTotal + DeliveryFee + ServiceFee.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Dirección de entrega confirmada (texto para mostrar al cliente).</summary>
        public string DeliveryAddressText { get; set; } = string.Empty;

        // ─── Factory Methods ────

        public static ConfirmOrderGroupResult Success(
            int orderGroupId,
            decimal subTotal,
            decimal deliveryFee,
            decimal serviceFee,
            decimal totalAmount,
            string addressText) => new()
            {
                OrderGroupId = orderGroupId,
                Status = "CapacityValidated",
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                ServiceFee = serviceFee,
                TotalAmount = totalAmount,
                DeliveryAddressText = addressText
            };
    }
}
