using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    // ───────────────────────────────────────────────────────────────────────
    // DTO: DriverActionResultDto
    // USO: Respuesta genérica para acciones del driver (Accept, UpdateStop).
    //      Evita crear un DTO por cada acción simple.
    // ───────────────────────────────────────────────────────────────────────
    public class DriverActionResultDto
    {
        /// <summary>Indica si la acción fue exitosa.</summary>
        public bool Success { get; set; }

        /// <summary>Mensaje descriptivo del resultado.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Nuevo estado del recurso afectado (OrderGroup o Stop).
        /// La app del driver actualiza su UI sin necesidad de otra llamada.
        /// </summary>
        public string? NewStatus { get; set; }
    }
}
