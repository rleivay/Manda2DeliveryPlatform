using Manda2.API.Extensions;
using Manda2.Application.Feature.Payments.Commands;
using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// BackOffice: validación manual de pagos por Transferencia.
    /// Ruta base: /api/backoffice/payments
    /// Roles: BackOffice, Admin.
    /// </summary>
    [Route("api/backoffice/payments")]
    [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
    public class BackOfficePaymentsController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public BackOfficePaymentsController(ICommandBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// POST /api/backoffice/payments/{paymentId}/confirm
        /// Valida manualmente un pago por Transferencia.
        /// Si el pago cubre el total del OrderGroup, dispara StartDispatch.
        /// </summary>
        [HttpPost("{paymentId:int}/confirm")]
        public async Task<IActionResult> ConfirmPayment(
            int paymentId,
            [FromBody] ConfirmPaymentRequest request,
            CancellationToken ct)
        {
            // ── OperatorId desde JWT claim "uid" ──────────────────────────
            int operatorId = User.GetUserId();

            var command = new ConfirmPaymentCommand(
                orderGroupPaymentId: paymentId,
                confirmedByUserId: operatorId,
                authorization: request.Authorization,
                reference: request.Reference,
                notes: request.Notes);

            var result = await _bus.SendAsync<ConfirmPaymentCommand, ConfirmPaymentResult>(command, ct);

            return Ok(new
            {
                result.OrderGroupId,
                result.OrderGroupPaymentId,
                result.DispatchStarted,
                result.Message
            });
        }
    }

    /// <summary>DTO de entrada para confirmación manual de Transferencia.</summary>
    public class ConfirmPaymentRequest
    {
        /// <summary>Número de autorización bancaria. Requerido.</summary>
        public string Authorization { get; set; } = string.Empty;

        /// <summary>Referencia bancaria final. Opcional.</summary>
        public string? Reference { get; set; }

        /// <summary>Notas del operador. Opcional.</summary>
        public string? Notes { get; set; }
    }
}