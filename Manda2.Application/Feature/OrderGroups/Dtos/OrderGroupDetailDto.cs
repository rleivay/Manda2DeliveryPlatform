// PROPÓSITO: DTO de respuesta para GetOrderGroupQuery.
//            Usado por Cliente (tracking), Driver (ruta activa) y BackOffice.
// ════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Dtos
{
    /// <summary>
    /// Vista completa del OrderGroup: estado, driver, subórdenes y paradas.
    /// </summary>
    public class OrderGroupDetailDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;

        // ─── Cliente ────
        public int CustomerId { get; set; }

        // ─── Driver (null si aún no asignado) ────
        public int? DriverId { get; set; }
        public string DriverName { get; set; } = "Asignando...";
        public string? DriverPhone { get; set; }

        // ─── Financiero ────
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public string? PaymentMethodName { get; set; }

        // ─── Dirección de entrega ────
        public string DeliveryAddressText { get; set; } = string.Empty;

        // ─── Subórdenes (por comercio) ────
        public List<SubOrderDetailDto> SubOrders { get; set; } = new();

        // ─── Paradas logísticas ────
        public List<OrderStopDto> Stops { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    /// <summary>
    /// Resumen de una SubOrder dentro del detalle del grupo.
    /// </summary>
    public class SubOrderDetailDto
    {
        public int SubOrderId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        /// <summary>SubTotal confirmado en la SubOrder (campo real: SubTotal).</summary>
        public decimal SubTotal { get; set; }
    }

    /// <summary>
    /// Parada logística dentro del detalle del grupo.
    /// </summary>
    public class OrderStopDto
    {
        public int StopId { get; set; }
        public int Sequence { get; set; }

        /// <summary>Tipo: "Pickup" o "Dropoff".</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Texto de dirección (campo real: AddressText).</summary>
        public string AddressText { get; set; } = string.Empty;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }
}
