using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Enum
{
    public class DispatchEnums
    {
        public enum DriverDispatchResponse
        {
            Pending = 1,
            Accepted = 2,
            Rejected = 3,
            Expired = 4
        }

       

        public enum StopType
        {
            Pickup = 1,
            Dropoff = 2
        }

        

        public enum DispatchAttemptStatus
        {
            Sent = 1,                  // Oferta enviada al driver, esperando respuesta
            Accepted = 2,              // Este driver aceptó el pedido
            Expired = 3,               // Timeout individual o ronda completa sin aceptación
            CancelledByAssignment = 4  // Otro driver aceptó primero — oferta neutralizada
        }

        /// <summary>
        /// Estrategia de selección de driver por ronda de dispatch.
        /// Determina cómo el DispatchStrategyResolver pondera los factores de scoring.
        /// </summary>
        public enum DispatchRoundStrategy
        {
            /// <summary>
            /// Ronda 1 (normal): Balancea fairness, distancia, SLA y confiabilidad.
            /// Pesos estándar definidos en DispatchConfig.
            /// </summary>
            FairnessFirst = 0,

            /// <summary>
            /// Ronda 2+: Prioriza SLA y distancia sobre fairness.
            /// Se activa cuando hay al menos 1 ronda fallida previa.
            /// </summary>
            SlaFirst = 1,

            /// <summary>
            /// Modo rescate: Ignora fairness completamente.
            /// Se activa cuando el tiempo restante al SLA es menor a RescueModeThresholdMinutes.
            /// Timeout reducido a UrgentRoundTimeoutSeconds.
            /// </summary>
            Rescue = 2
        }

    }
}
