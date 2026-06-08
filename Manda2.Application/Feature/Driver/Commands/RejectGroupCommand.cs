// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/RejectGroup/
//          RejectGroupCommand.cs
//
// PROPÓSITO: Define el contrato de entrada para rechazar una oferta de pedido.
//            El driver presiona "Rechazar" en la app y este comando viaja
//            al handler con los datos mínimos necesarios.
//
// PATRÓN: ICommand<TResult> del mediador propio (sin MediatR).
// CONSUMIDOR: DriverController → POST /api/driver/groups/{id}/reject
// ═══════════════════════════════════════════════════════════════════════════
// using System;
using Manda2.Application.Mediator;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Comando que representa la acción de un repartidor de rechazar
    /// explícitamente una oferta de grupo de pedidos.
    /// </summary>
    public class RejectGroupCommand : ICommand<RejectGroupResult>
    {
        /// <summary>
        /// ID del grupo de órdenes que el driver está rechazando.
        /// </summary>
        public int OrderGroupId { get; set; }

        /// <summary>
        /// ID del repartidor que rechaza la oferta.
        /// Se obtiene del JWT token en el controller, no del body del request.
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        /// ID del motivo seleccionado del catálogo DriverRejectionReason.
        /// Obligatorio. La app MAUI carga este catálogo al abrir la pantalla.
        /// </summary>
        public int ReasonId { get; set; }

        /// <summary>
        /// Texto libre del driver. Obligatorio solo cuando el motivo
        /// seleccionado tiene RequiresNote = true (ej: "Otro motivo").
        /// La app MAUI activa/desactiva este campo dinámicamente.
        /// </summary>
        public string? ReasonNotes { get; set; }
    }
}
