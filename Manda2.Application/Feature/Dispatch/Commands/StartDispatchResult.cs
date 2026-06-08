using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Resultado devuelto por el handler de StartDispatch.
    /// </summary>
    public class StartDispatchResult
    {
        /// <summary>Id del DispatchAttempt creado.</summary>
        public int AttemptId { get; set; }

        /// <summary>Número de drivers notificados en esta ronda.</summary>
        public int NotifiedDriversCount { get; set; }

        /// <summary>Timestamp UTC de expiración de la ronda.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>Mensaje descriptivo del resultado (útil para logs y BackOffice).</summary>
        public string Message { get; set; } = string.Empty;
    }
}
