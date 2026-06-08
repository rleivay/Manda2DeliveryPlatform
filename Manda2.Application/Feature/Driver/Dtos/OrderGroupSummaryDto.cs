using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: OrderGroupSummaryDto
    // USO: Respuesta del endpoint GET /api/driver/available-orders
    //      El driver ve una lista de grupos disponibles cercanos a él.
    // ───────────────────────────────────────────────────────────────────────
    public class OrderGroupSummaryDto
    {
        /// <summary>ID del grupo de órdenes (multi-comercio)</summary>
        public int OrderGroupId { get; set; }

        /// <summary>
        /// Número de comercios (SubOrders) dentro del grupo.
        /// Ej: "2 comercios" → el driver hará 2 pickups + 1 dropoff.
        /// </summary>
        public int MerchantCount { get; set; }

        /// <summary>
        /// Distancia estimada en km desde la posición actual del driver
        /// hasta el primer punto de recogida.
        /// Calculada por el DispatchService usando Haversine.
        /// </summary>
        public decimal EstimatedDistanceKm { get; set; }

        /// <summary>
        /// Tiempo estimado total de la ruta completa en minutos.
        /// Incluye: viaje a comercios + espera + viaje al cliente.
        /// </summary>
        public int EstimatedTotalMinutes { get; set; }

        /// <summary>
        /// Monto total del pedido (suma de todas las SubOrders).
        /// El driver lo ve para saber cuánto efectivo podría recibir.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Método de pago del cliente.
        /// Crítico: si es "Efectivo", el driver debe tener capacidad de cambio.
        /// </summary>
        public string PaymentMethodName { get; set; } = string.Empty;

        /// <summary>
        /// Dirección de entrega final (texto legible para el driver).
        /// </summary>
        public string DeliveryAddressText { get; set; } = string.Empty;

        /// <summary>
        /// Lista resumida de paradas en orden de secuencia.
        /// El driver ve: Pickup en "Comercio A" → Pickup en "Comercio B" → Dropoff.
        /// </summary>
        public List<StopSummaryDto> Stops { get; set; } = new();

        /// <summary>
        /// Segundos restantes para que expire esta oferta de dispatch.
        /// La app del driver muestra un countdown timer.
        /// </summary>
        public int ExpiresInSeconds { get; set; }
    }
}
