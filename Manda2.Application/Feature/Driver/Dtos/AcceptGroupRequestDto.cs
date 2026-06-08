using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: AcceptGroupRequestDto
    // USO: Body del endpoint POST /api/driver/accept-group/{orderGroupId}
    //      El driver confirma que acepta la ruta.
    // ───────────────────────────────────────────────────────────────────────
    public class AcceptGroupRequestDto
    {
        /// <summary>
        /// Latitud actual del driver al momento de aceptar.
        /// Se guarda para auditoría y para recalcular ETA si cambió.
        /// </summary>
        public decimal CurrentLatitude { get; set; }

        /// <summary>Longitud actual del driver al momento de aceptar.</summary>
        public decimal CurrentLongitude { get; set; }
    }
}
