// DIFERENCIA vs AcceptDispatchCommand:
//   - AcceptDispatchCommand: usado por el sistema/BackOffice para forzar
//     asignación (ej: background service, override manual).
//   - AcceptGroupCommand: usado por el Driver desde su app.
//     Incluye coordenadas GPS de aceptación y valida que la oferta
//     esté dirigida específicamente a este driver.
//
// CONSUMIDOR: DriversController → POST api/drivers/groups/accept

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Comando driver-side: el repartidor acepta una oferta de pedido.
    /// </summary>
    public record AcceptGroupCommand(
        int DriverId,
        int OrderGroupId,
        decimal Latitude,
        decimal Longitude
    ) : ICommand<AcceptGroupResult>; 
}
