// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.API/Controllers/DriversController.cs
//
// PROPÓSITO: Entry point REST para todas las operaciones del Repartidor.
//            Cubre el ciclo de vida completo:
//            - Registro y aprobación (BackOffice)
//            - Consulta de ofertas disponibles (App Driver)
//            - Aceptar / Rechazar una oferta (App Driver)
//            - Flujo de ejecución de ruta (App Driver) ← próximos sprints
//
// PATRÓN:
//   - Hereda de BaseApiController para respuestas HTTP estandarizadas.
//   - Inyecta ICommandBus (mediador propio) para desacoplar lógica de negocio.
//   - El controlador NO contiene lógica. Solo recibe, delega y responde.
//
// SEGURIDAD (TODO Sprint 4):
//   - Los endpoints de App Driver deben decorarse con [Authorize(Roles = "Driver")]
//   - El DriverId debe extraerse del JWT claim, no del body/query.
//   - Los endpoints de BackOffice deben decorarse con [Authorize(Roles = "BackOffice")]
//
// INTEGRACIÓN SAP B1 (TODO Sprint 5):
//   - CompleteDelivery disparará la creación del documento en SAP B1
//     a través del SapB1Service inyectado en el Handler correspondiente.
//
// RUTA BASE: api/drivers
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.API.Extensions;
using Manda2.Application.Drivers;
using Manda2.Application.DTOs;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Application.Feature.Driver.Commands;
using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Feature.Driver.Queries;
using Manda2.Application.Mediator;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// Entry point REST para todas las operaciones del Repartidor.
    /// Ruta base: api/drivers
    /// </summary>
    [Authorize]
    public class DriversController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public DriversController(ICommandBus bus)
        {
            _bus = bus;
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECCIÓN 1: GESTIÓN (BackOffice)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET api/drivers/{id}
        /// Driver: solo puede ver su propio perfil.
        /// BackOffice/Admin: puede ver cualquier perfil.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{AppRoles.Driver},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            // ── Driver solo ve su propio perfil ───────────────────────────
            if (User.IsInRole(AppRoles.Driver))
            {
                int? driverId = User.GetDriverId();
                if (driverId == null || driverId.Value != id)
                    return Forbid();
            }

            var result = await _bus.QueryAsync<GetDriverByIdQuery, DriverDto?>(
                new GetDriverByIdQuery(id), ct);
            return OkOrNotFound(result);
        }

        /// <summary>
        /// POST api/drivers
        /// Registro público — no requiere token.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(
            [FromBody] CreateDriverCommand command, CancellationToken ct)
        {
            await _bus.SendAsync<CreateDriverCommand, Unit>(command, ct);
            return Ok(new { Message = "Repartidor registrado. Pendiente de aprobación por BackOffice." });
        }

        /// <summary>
        /// PATCH api/drivers/{id}/approve
        /// Solo BackOffice/Admin puede aprobar repartidores.
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> Approve(
            int id, [FromBody] ApproveDriverCommand cmd, CancellationToken ct)
        {
            if (id != cmd.DriverId)
                return BadRequest("El ID de la ruta no coincide con el del comando.");

            await _bus.SendAsync<ApproveDriverCommand, Unit>(cmd, ct);
            return Ok(new { Message = "Repartidor aprobado exitosamente." });
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECCIÓN 2: DISPATCH — OFERTAS (App Driver)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// GET api/drivers/offers?lat=14.5897&lon=-90.5123
        /// Retorna ofertas disponibles para el driver autenticado.
        /// DriverId extraído del JWT — ya no se acepta en query string.
        /// </summary>
        [HttpGet("offers")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> GetPendingOffers(
            [FromQuery] decimal lat,
            [FromQuery] decimal lon,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            var query = new GetPendingOffersQuery
            {
                DriverId = driverId.Value,
                CurrentLatitude = lat,
                CurrentLongitude = lon
            };

            var result = await _bus.QueryAsync<GetPendingOffersQuery, List<PendingOfferDto>>(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// POST api/drivers/groups/accept
        ///
        /// D-10 RESUELTO: Este endpoint usa AcceptGroupCommand (driver-side),
        /// separado del AcceptDispatchCommand (system-side).
        /// El DriverId se extrae del JWT — no se acepta en el body.
        /// </summary>
        [HttpPost("groups/accept")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> AcceptGroup(
            [FromBody] AcceptGroupRequest request,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            // AcceptGroupCommand: comando driver-side (ver archivo separado abajo)
            var command = new AcceptGroupDriverCommand(
                DriverId: driverId.Value,
                OrderGroupId: request.OrderGroupId,
                CurrentLatitude: request.Latitude,
                CurrentLongitude: request.Longitude);

            var result = await _bus.SendAsync<AcceptGroupDriverCommand, AcceptGroupDriverResult>(command, ct);

            return result.Success ? Ok(result) : BadRequest(new { result.Message });
        }

        /// <summary>
        /// POST api/drivers/groups/reject
        /// DriverId extraído del JWT.
        /// </summary>
        [HttpPost("groups/reject")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> RejectGroup(
            [FromBody] RejectGroupRequest request,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            var command = new RejectGroupCommand
            {
                DriverId = driverId.Value,
                OrderGroupId = request.OrderGroupId,
                ReasonId = request.ReasonId,
                ReasonNotes = request.ReasonNotes
            };

            var result = await _bus.SendAsync<RejectGroupCommand, RejectGroupResult>(command, ct);

            return result.Success
                ? Ok(result)
                : BadRequest(new { result.Message });
        }

        /// <summary>
        /// GET api/drivers/rejection-reasons
        /// Catálogo de motivos de rechazo activos para el Picker de la app.
        /// </summary>
        [HttpGet("rejection-reasons")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> GetRejectionReasons(CancellationToken ct)
        {
            var result = await _bus.QueryAsync<GetRejectionReasonsQuery, List<RejectionReasonDto>>(
                new GetRejectionReasonsQuery(), ct);
            return Ok(result);
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECCIÓN 3: TRACKING / GPS (App Driver)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/drivers/location
        /// Actualiza la ubicación GPS del driver autenticado.
        /// DriverId extraído del JWT — no se acepta en la ruta ni en el body.
        /// </summary>
        [HttpPost("location")]
        [Authorize(Roles = AppRoles.Driver)]
        public async Task<IActionResult> UpdateLocation(
            [FromBody] UpdateLocationRequest request,
            CancellationToken ct)
        {
            // ── DriverId desde JWT claim "did" ────────────────────────────
            int? driverId = User.GetDriverId();
            if (driverId == null)
                return Unauthorized("Token no contiene un DriverId válido.");

            var command = new UpdateDriverLocationCommand
            {
                DriverId = driverId.Value,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            var result = await _bus.SendAsync<UpdateDriverLocationCommand, UpdateDriverLocationResult>(command, ct);

            return result.Success ? Ok(result) : NotFound(result.Message);
        }
    }

    // ─── DTOs de Request ──────────────────────────────────────────────────

    /// <summary>Payload para POST api/drivers/groups/accept (D-10)</summary>
    public record AcceptGroupRequest(
        int OrderGroupId,
        decimal Latitude,
        decimal Longitude
    );

    /// <summary>Payload para POST api/drivers/groups/reject</summary>
    public record RejectGroupRequest(
        int OrderGroupId,
        int ReasonId,
        string? ReasonNotes = null
    );

    /// <summary>Payload para POST api/drivers/location</summary>
    public record UpdateLocationRequest(decimal Latitude, decimal Longitude);
}