// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/ArriveAtStop/
//          ArriveAtStopCommand.cs
//
// PROPÓSITO: Representa la acción del repartidor de marcar su llegada
//            a una parada de la ruta (comercio o cliente).
//
//            Esta acción:
//              - Registra el timestamp real de llegada (ArrivedAt).
//              - Permite comparar vs EstimatedArrivalAt para métricas de SLA.
//              - Habilita el siguiente paso: PickedUp (si es Pickup)
//                o Delivered (si es Dropoff).
//
// PATRÓN: ICommand<TResult> del mediador propio (ICommandBus).
// CONSUMIDOR: DriversController → POST api/drivers/stops/{id}/arrive
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;

namespace Manda2.Application.Feature.Driver.Commands.ArriveAtStop
{
    /// <summary>
    /// Comando que registra la llegada física del repartidor a una parada.
    /// Aplica tanto a paradas tipo Pickup (comercio) como Dropoff (cliente).
    /// </summary>
    public class ArriveAtStopCommand : ICommand<ArriveAtStopResult>
    {
        /// <summary>
        /// ID de la parada (OrderGroupStop.Id) a la que llegó el driver.
        /// </summary>
        public int StopId { get; set; }

        /// <summary>
        /// ID del repartidor que ejecuta la acción.
        /// TODO Sprint 4: extraer del JWT claim en el controller.
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// Posición GPS real del driver al momento de marcar llegada.
        /// Permite validar proximidad y detectar fraudes de geolocalización.
        /// </summary>
        public decimal CurrentLatitude { get; set; }
        public decimal CurrentLongitude { get; set; }
    }
}