using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: UpdateStopRequestDto
    // USO: Body del endpoint POST /api/driver/update-stop
    //      El driver marca una parada como completada.
    //      Ej: "Llegué al comercio", "Recogí el pedido", "Entregué al cliente".
    // ───────────────────────────────────────────────────────────────────────
    public class UpdateStopRequestDto
    {
        /// <summary>ID de la parada (OrderGroupStop) que se está completando.</summary>
        public int StopId { get; set; }

        /// <summary>
        /// Nuevo estado de la parada.
        /// Valores válidos: "ArrivedAtMerchant", "PickedUp", "Delivered"
        /// Se valida en el Command Handler.
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// Latitud GPS del driver al completar la parada.
        /// Se guarda para auditoría y verificación de proximidad.
        /// </summary>
        public decimal CurrentLatitude { get; set; }

        /// <summary>Longitud GPS del driver al completar la parada.</summary>
        public decimal CurrentLongitude { get; set; }

        /// <summary>
        /// Notas opcionales del driver.
        /// Ej: "El cliente no estaba, dejé con portero".
        /// </summary>
        public string? Notes { get; set; }
    }
}
