using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Comando de aceptación de oferta de despacho por parte del driver.
    /// Las coordenadas GPS son obligatorias para auditoría: permiten verificar
    /// que el driver estaba físicamente cerca al momento de aceptar.
    /// Se almacenan en OrderGroup.DriverLatAtAcceptance / DriverLonAtAcceptance.
    /// </summary>
    public record AcceptDispatchCommand(
        int DriverId,
        int OrderGroupId,
        decimal DriverLatitude,
        decimal DriverLongitude
    ) : ICommand<AcceptDispatchResult>;
}
