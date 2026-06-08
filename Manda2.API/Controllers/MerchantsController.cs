using Manda2.API.Extensions;
using Manda2.Application.DTOs;
using Manda2.Application.Feature.Merchant.Commands;
using Manda2.Application.Feature.Merchant.Dtos;
using Manda2.Application.Feature.Merchant.Queries;
using Manda2.Application.Mediator;
using Manda2.Application.Merchants;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    [Authorize]
    public class MerchantsController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public MerchantsController(ICommandBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// GET api/merchants/{id}
        /// Merchant: solo puede ver su propio perfil.
        /// BackOffice/Admin: puede ver cualquier perfil.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{AppRoles.Merchant},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            // ── Merchant solo ve su propio perfil ─────────────────────────
            if (User.IsInRole(AppRoles.Merchant))
            {
                int? merchantId = User.GetMerchantId();
                if (merchantId == null || merchantId.Value != id)
                    return Forbid();
            }

            var result = await _bus.QueryAsync<GetMerchantByIdQuery, MerchantDto?>(
                new GetMerchantByIdQuery(id), ct);
            return OkOrNotFound(result);
        }

        /// <summary>
        /// POST api/merchants
        /// Registro público — no requiere token.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(
            [FromBody] CreateMerchantCommand command, CancellationToken ct)
        {
            await _bus.SendAsync<CreateMerchantCommand, Unit>(command, ct);
            return Ok(new { Message = "Merchant creado (pendiente validación BackOffice)" });
        }

        /// <summary>
        /// PATCH api/merchants/{id}/approve
        /// Solo BackOffice/Admin puede aprobar comercios.
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> Approve(
            int id, [FromBody] ApproveMerchantCommand cmd, CancellationToken ct)
        {
            if (id != cmd.MerchantId) return BadRequest("IDs no coinciden.");
            await _bus.SendAsync<ApproveMerchantCommand, Unit>(cmd, ct);
            return Ok(new { Message = "Merchant aprobado exitosamente" });
        }

        // ═══════════════════════════════════════════════════════════════════
        // SECCIÓN: GESTIÓN DE SUBÓRDENES (App Merchant)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// POST api/merchants/suborders/{id}/accept
        /// MerchantId extraído del JWT — no se acepta en el body.
        /// </summary>
        [HttpPost("suborders/{id:int}/accept")]
        [Authorize(Roles = AppRoles.Merchant)]
        public async Task<IActionResult> AcceptSubOrder(int id, CancellationToken ct)
        {
            // ── MerchantId desde JWT claim "mid" ──────────────────────────
            int? merchantId = User.GetMerchantId();
            if (merchantId == null)
                return Unauthorized("Token no contiene un MerchantId válido.");

            var result = await _bus.SendAsync<AcceptSubOrderCommand, AcceptSubOrderResult>(
                new AcceptSubOrderCommand
                {
                    SubOrderId = id,
                    MerchantId = merchantId.Value
                }, ct);

            return result.Success ? Ok(result) : BadRequest(new { result.Message });
        }

        /// <summary>
        /// POST api/merchants/suborders/{id}/reject
        /// MerchantId extraído del JWT.
        /// </summary>
        [HttpPost("suborders/{id:int}/reject")]
        [Authorize(Roles = AppRoles.Merchant)]
        public async Task<IActionResult> RejectSubOrder(
            int id,
            [FromBody] RejectSubOrderRequest request,
            CancellationToken ct)
        {
            // ── MerchantId desde JWT claim "mid" ──────────────────────────
            int? merchantId = User.GetMerchantId();
            if (merchantId == null)
                return Unauthorized("Token no contiene un MerchantId válido.");

            var result = await _bus.SendAsync<RejectSubOrderCommand, RejectSubOrderResult>(
                new RejectSubOrderCommand
                {
                    SubOrderId = id,
                    MerchantId = merchantId.Value,
                    RejectReason = request.RejectReason
                }, ct);

            return result.Success ? Ok(result) : BadRequest(new { result.Message });
        }

        /// <summary>
        /// POST api/merchants/suborders/{id}/ready
        /// MerchantId extraído del JWT.
        /// </summary>
        [HttpPost("suborders/{id:int}/ready")]
        [Authorize(Roles = AppRoles.Merchant)]
        public async Task<IActionResult> MarkReady(int id, CancellationToken ct)
        {
            // ── MerchantId desde JWT claim "mid" ──────────────────────────
            int? merchantId = User.GetMerchantId();
            if (merchantId == null)
                return Unauthorized("Token no contiene un MerchantId válido.");

            var result = await _bus.SendAsync<MarkSubOrderReadyCommand, MarkSubOrderReadyResult>(
                new MarkSubOrderReadyCommand
                {
                    SubOrderId = id,
                    MerchantId = merchantId.Value
                }, ct);

            return result.Success ? Ok(result) : BadRequest(new { result.Message });
        }

        /// <summary>
        /// GET api/merchants/{merchantId}/suborders?statusFilter=Preparing
        /// Merchant: solo puede ver sus propias subórdenes (validado contra JWT).
        /// BackOffice/Admin: puede ver cualquier comercio.
        /// </summary>
        [HttpGet("{merchantId:int}/suborders")]
        [Authorize(Roles = $"{AppRoles.Merchant},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> GetSubOrders(
            int merchantId,
            [FromQuery] string? statusFilter,
            CancellationToken ct)
        {
            // ── Merchant solo ve sus propias subórdenes ───────────────────
            if (User.IsInRole(AppRoles.Merchant))
            {
                int? jwtMerchantId = User.GetMerchantId();
                if (jwtMerchantId == null || jwtMerchantId.Value != merchantId)
                    return Forbid();
            }

            var result = await _bus.QueryAsync<GetMerchantSubOrdersQuery, List<MerchantSubOrderDto>>(
                new GetMerchantSubOrdersQuery
                {
                    MerchantId = merchantId,
                    StatusFilter = statusFilter
                }, ct);

            return Ok(result);
        }

        // ─── DTOs de Request ──────────────────────────────────────────────
        public record RejectSubOrderRequest(string? RejectReason);
    }
}
