using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: ActiveStopDto
    // USO: Parada con estado dentro de ActiveRouteDto.
    // ───────────────────────────────────────────────────────────────────────
    public class ActiveStopDto
    {
        public int StopId { get; set; }
        public int Sequence { get; set; }
        public string StopType { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string AddressText { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        /// <summary>
        /// Estado actual de la parada.
        /// Pending → ArrivedAtMerchant → PickedUp → Delivered
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Indica si esta es la próxima parada a completar.
        /// La app del driver resalta esta parada en el mapa.
        /// </summary>
        public bool IsNext { get; set; }

        /// <summary>Notas del driver si ya fue completada.</summary>
        public string? Notes { get; set; }
    }
}
