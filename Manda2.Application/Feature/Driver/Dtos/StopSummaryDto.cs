using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: StopSummaryDto
    // USO: Parada individual dentro de OrderGroupSummaryDto.
    //      Representa un Pickup (comercio) o Dropoff (cliente).
    // ───────────────────────────────────────────────────────────────────────
    public class StopSummaryDto
    {
        /// <summary>Número de orden de la parada en la ruta (1, 2, 3...)</summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Tipo de parada.
        /// "Pickup" = recoger en comercio.
        /// "Dropoff" = entregar al cliente.
        /// </summary>
        public string StopType { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del comercio (solo en Pickup) o "Entrega al cliente" (Dropoff).
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Dirección legible de la parada.</summary>
        public string AddressText { get; set; } = string.Empty;

        /// <summary>Latitud GPS de la parada.</summary>
        public decimal Latitude { get; set; }

        /// <summary>Longitud GPS de la parada.</summary>
        public decimal Longitude { get; set; }
    }
}
