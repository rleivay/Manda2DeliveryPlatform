using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Dtos
{
    /// <summary>
    /// Resumen de un OrderGroup para listados paginados en BackOffice.
    /// </summary>
    public class OrderGroupSummaryDto
    {
        public int Id { get; set; }

        /// <summary>Estado del grupo (ej: "AwaitingDriverAssignment", "InProgress").</summary>
        public string Status { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        /// <summary>Nombre completo del cliente (proyectado desde Customer).</summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>DriverId asignado. Null si aún no hay driver.</summary>
        public int? DriverId { get; set; }

        /// <summary>Nombre completo del driver. Null si no asignado.</summary>
        public string? DriverName { get; set; }

        public int SubOrderCount { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal ServiceFee { get; set; }

        /// <summary>Nombre del método de pago (snapshot al momento de crear).</summary>
        public string? PaymentMethodName { get; set; }

        public string DeliveryAddressText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        /// <summary>Fecha de entrega. Null si aún no entregado.</summary>
        public DateTime? DeliveredAt { get; set; }
    }
}
