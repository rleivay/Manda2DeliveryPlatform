// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/ArriveAtStop/
//          ArriveAtStopResult.cs
//
// PROPÓSITO: Contrato de salida del comando ArriveAtStop.
//            La app del driver usa este resultado para:
//              - Confirmar la llegada visualmente.
//              - Saber qué acción mostrar a continuación
//                (botón "Recolectar" o botón "Entregar").
//              - Conocer el tipo de parada para adaptar la UI.
// ═══════════════════════════════════════════════════════════════════════════

using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Driver.Commands.ArriveAtStop
{
    /// <summary>
    /// Resultado del comando ArriveAtStopCommand.
    /// </summary>
    public class ArriveAtStopResult
    {
        /// <summary>
        /// Indica si la llegada fue registrada exitosamente.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado para mostrar en la app.
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Tipo de parada: Pickup o Dropoff.
        /// La app usa este valor para mostrar el botón correcto:
        ///   Pickup  → "Confirmar Recolección"
        ///   Dropoff → "Confirmar Entrega"
        /// </summary>
        public StopType StopType { get; set; }

        /// <summary>
        /// Timestamp UTC en que se registró la llegada.
        /// </summary>
        public DateTime ArrivedAtUtc { get; set; }
    }
}