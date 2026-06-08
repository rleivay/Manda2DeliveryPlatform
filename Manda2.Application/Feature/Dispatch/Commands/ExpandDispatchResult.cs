using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    // ────────────────────────────────────────────────────────────────────────
    // RESULT
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Resultado de la expansión de dispatch.</summary>
    public class ExpandDispatchResult
    {
        /// <summary>True si se creó una nueva ronda exitosamente.</summary>
        public bool NewAttemptCreated { get; set; }

        /// <summary>ID del nuevo DispatchAttempt creado. Null si no se creó.</summary>
        public int? NewAttemptId { get; set; }

        /// <summary>Número de ronda del nuevo intento. 0 si no se creó.</summary>
        public int NewRoundNumber { get; set; }

        /// <summary>Radio de búsqueda aplicado en la nueva ronda (km).</summary>
        public decimal RadiusUsedKm { get; set; }

        /// <summary>Cantidad de drivers notificados en la nueva ronda.</summary>
        public int DriversNotified { get; set; }

        /// <summary>Mensaje descriptivo del resultado para logs y auditoría.</summary>
        public string Message { get; set; } = null!;
    }

}
