// File: CompleteStopCommand.cs
// Namespace: Manda2.Application.Features.Dispatch.Commands.CompleteStop
//
// Comando que representa la acción del driver al marcar una parada como completada.
// NOTA: Este comando asume la existencia del mediador propio del proyecto.
// Ajusta el tipo ICommand<TResult> si tu mediador usa otra convención.

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Comando que solicita completar una parada (OrderGroupStop).
    /// - StopId: Id de la parada
    /// - DriverId: Id del repartidor (obtenido del JWT en el controller)
    /// - Notes: Nota opcional del repartidor (ej. "cliente ausente, dejé con portero")
    /// </summary>
    public class CompleteStopCommand : ICommand<CompleteStopResult>
    {
        public int StopId { get; }
        public int DriverId { get; }
        public string? Notes { get; }

        public CompleteStopCommand(int stopId, int driverId, string? notes = null)
        {
            StopId = stopId;
            DriverId = driverId;
            Notes = notes;
        }
    }

    
}
