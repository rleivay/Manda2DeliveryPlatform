using Manda2.API.Extensions;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Application.Feature.Driver.Commands;
using Manda2.Application.Mediator;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// Motor de Dispatch — operaciones del sistema y del driver.
    /// Ruta base: /api/dispatch
    /// </summary>
    [Route("api/dispatch")]
    [ApiController]
    [Authorize]
    public class DispatchController : BaseApiController
    {
        private readonly ICommandBus _commandBus;

        public DispatchController(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /api/dispatch/start
        // Roles: BackOffice, Admin (también llamado por background service).
        // ─────────────────────────────────────────────────────────────────
        /// <summary>
        /// Inicia una ronda de dispatch para un OrderGroup.
        /// Crea un DispatchAttempt y notifica a los drivers candidatos.
        /// </summary>
        [HttpPost("start")]
        [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> StartDispatch(
            [FromBody] StartDispatchRequest request,
            CancellationToken ct)
        {
            // ── initiatedByUserId desde JWT ───────────────────────────────
            int userId = User.GetUserId();

            var command = new StartDispatchCommand(
                orderGroupId: request.OrderGroupId,
                initiatedByUserId: userId > 0 ? userId : null,
                zoneName: request.ZoneName);

            var result = await _commandBus
                .SendAsync<StartDispatchCommand, StartDispatchResult>(command, ct);

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /api/dispatch/stops/{stopId}/complete
        // Rol: Driver — solo el driver asignado puede completar paradas.
        // ─────────────────────────────────────────────────────────────────
        /// <summary>
        /// Marca una parada (Pickup o Dropoff) como completada.
        /// El handler valida que el driver del JWT sea el asignado al grupo.
        /// </summary>
        [HttpPost("stops/{stopId:int}/complete")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> CompleteStop(
            [FromRoute] int stopId,
            [FromBody] CompleteStopRequest? request,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            // Notes es opcional: el driver puede dejar una nota de entrega
            // (ej. "dejé con portero", "cliente ausente — entregué a vecino").
            var command = new CompleteStopCommand(stopId, driverId.Value, request?.Notes);

            var result = await _commandBus
                .SendAsync<CompleteStopCommand, CompleteStopResult>(command, ct);

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /api/dispatch/groups/{groupId}/accept
        // Rol: Driver — solo el driver notificado puede aceptar la oferta.
        // ─────────────────────────────────────────────────────────────────
        /// <summary>
        /// El driver acepta una oferta de grupo de pedidos.
        /// Recibe las coordenadas GPS del driver al momento de aceptar
        /// para auditoría de proximidad y SLA.
        /// Transiciona: AssignedToDriver → DriverAccepted.
        /// Neutraliza todos los DispatchAttempts activos del grupo (CancelledByAssignment).
        /// </summary>
        [HttpPost("groups/{groupId:int}/accept")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> AcceptDispatch(
            [FromRoute] int groupId,
            [FromBody] AcceptDispatchRequest request,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            var command = new AcceptDispatchCommand(
                DriverId: driverId.Value,
                OrderGroupId: groupId,
                DriverLatitude: request.Latitude,
                DriverLongitude: request.Longitude);

            var result = await _commandBus
                .SendAsync<AcceptDispatchCommand, AcceptDispatchResult>(command, ct);

            return result.Success
                ? Ok(new { result.Success, result.Message, result.OrderGroupId })
                : BadRequest(new { result.Message });
        }

        // ─────────────────────────────────────────────────────────────────
        // POST /api/dispatch/groups/{groupId}/reject
        // Rol: Driver.
        // ─────────────────────────────────────────────────────────────────
        /// <summary>
        /// El driver rechaza explícitamente una oferta.
        /// El motor evaluará si saltar a la siguiente ronda.
        /// </summary>
        [HttpPost("groups/{groupId:int}/reject")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> RejectDispatch(
            [FromRoute] int groupId,
            [FromBody] RejectDispatchRequest request,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            var command = new RejectGroupCommand
            {
                DriverId = driverId.Value,
                OrderGroupId = groupId,
                ReasonId = request.ReasonId,
                ReasonNotes = request.ReasonNotes
            };

            var result = await _commandBus
                .SendAsync<RejectGroupCommand, RejectGroupResult>(command, ct);

            return result.Success
                ? Ok(new { result.Success, result.Message })
                : BadRequest(new { result.Message });
        }
    }

    // ─── DTOs de Request ──────────────────────────────────────────────────

    /// <summary>Payload para POST /api/dispatch/start</summary>
    public record StartDispatchRequest(int OrderGroupId, string? ZoneName = null);

    /// <summary>
    /// Payload para POST /api/dispatch/groups/{groupId}/accept.
    /// Las coordenadas GPS son obligatorias — la app MAUI las obtiene
    /// del GPS del dispositivo en el momento exacto en que el driver presiona "Aceptar".
    /// </summary>
    public record AcceptDispatchRequest(decimal Latitude, decimal Longitude);

    /// <summary>
    /// Payload para POST /api/dispatch/stops/{stopId}/complete.
    /// Notes es opcional — el driver puede agregar una observación de entrega.
    /// </summary>
    public record CompleteStopRequest(string? Notes = null);

    /// <summary>Payload para POST /api/dispatch/groups/{groupId}/reject</summary>
    public record RejectDispatchRequest(int ReasonId, string? ReasonNotes = null);
}
