// PROPÓSITO:
//   Define el comando que inicia una ronda de dispatch para un OrderGroup.

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Comando para iniciar el proceso de dispatch de un OrderGroup.
    /// Crea un DispatchAttempt (ronda 1) y notifica a los drivers candidatos
    /// dentro del radio configurado en AppConfig / DispatchConfig.
    /// </summary>
    public class StartDispatchCommand : ICommand<StartDispatchResult>
    {
        /// <summary>
        /// Identificador del OrderGroup que requiere asignación de driver.
        /// </summary>
        public int OrderGroupId { get; }

        /// <summary>
        /// Usuario que inicia la acción (BackOffice o sistema automático).
        /// Nullable: si es null se registra como "System" en AuditLog.
        /// </summary>
        public int? InitiatedByUserId { get; }

        /// <summary>
        /// Zona operacional para seleccionar la DispatchConfig correspondiente.
        /// Si es null se usa la configuración por defecto de AppConfig.
        /// </summary>
        public string? ZoneName { get; }

        public StartDispatchCommand(int orderGroupId, int? initiatedByUserId = null, string? zoneName = null)
        {
            OrderGroupId = orderGroupId;
            InitiatedByUserId = initiatedByUserId;
            ZoneName = zoneName;
        }
    }

}
