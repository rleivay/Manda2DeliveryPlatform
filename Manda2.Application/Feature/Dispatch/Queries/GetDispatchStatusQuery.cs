// PROPÓSITO: Consulta el estado actual del ciclo de dispatch de un OrderGroup.
//            Incluye ronda activa + historial de rondas anteriores.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Queries
{
    /// <summary>
    /// Retorna el estado completo del dispatch para un OrderGroup.
    /// Consumidores: BackOffice (panel de monitoreo), DispatchWorker (diagnóstico).
    /// </summary>
    public class GetDispatchStatusQuery : IQuery<DispatchStatusDto>
    {
        public int OrderGroupId { get; }

        public GetDispatchStatusQuery(int orderGroupId)
        {
            OrderGroupId = orderGroupId;
        }
    }
}
