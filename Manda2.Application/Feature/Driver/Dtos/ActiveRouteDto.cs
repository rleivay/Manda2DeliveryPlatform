using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: ActiveRouteDto
    // USO: Respuesta del endpoint GET /api/driver/active-route
    //      El driver ve su ruta activa completa con el estado de cada parada.
    // ───────────────────────────────────────────────────────────────────────
    public class ActiveRouteDto
    {
        /// <summary>ID del grupo de órdenes activo del driver.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>Estado general del grupo (InTransit, Delivering, etc.)</summary>
        public string GroupStatus { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del cliente final.
        /// El driver lo necesita para identificarse al entregar.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>Teléfono del cliente para contacto directo.</summary>
        public string CustomerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Lista completa de paradas con su estado actual.
        /// El driver ve cuáles completó y cuál es la siguiente.
        /// </summary>
        public List<ActiveStopDto> Stops { get; set; } = new();

        /// <summary>Monto total a cobrar si el pago es en efectivo.</summary>
        public decimal TotalAmountToCash { get; set; }

        /// <summary>Método de pago del cliente.</summary>
        public string PaymentMethodName { get; set; } = string.Empty;
    }
}
