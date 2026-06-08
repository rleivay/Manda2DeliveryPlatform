// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/AcceptGroup/
//          AcceptGroupCommand.cs
//
// PROPÓSITO: Contrato de entrada cuando el driver acepta un grupo de órdenes.
//
// CUÁNDO SE USA:
// - El driver presiona "Aceptar" en la app antes de que expire el countdown.
// - El endpoint POST /api/driver/accept-group invoca este command.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Command para que un driver acepte un grupo de órdenes disponible.
    /// </summary>
    /// <param name="DriverId">ID del driver que acepta.</param>
    /// <param name="OrderGroupId">ID del grupo que está aceptando.</param>
    /// <param name="CurrentLatitude">Posición GPS actual al momento de aceptar.</param>
    /// <param name="CurrentLongitude">Posición GPS actual al momento de aceptar.</param>
    public record AcceptGroupDriverCommand(
        int DriverId,
        int OrderGroupId,
        decimal CurrentLatitude,
        decimal CurrentLongitude
    ) : ICommand<AcceptGroupDriverResult>;
}
