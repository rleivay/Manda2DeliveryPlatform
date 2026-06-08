using Manda2.API.Extensions;
using Manda2.Application.Feature.Checkout.Commands;
using Manda2.Application.Feature.Checkout.Queries;
using Manda2.Application.Feature.OrderGroups.Commands;
using Manda2.Application.Feature.OrderGroups.Dtos;
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
    /// Checkout para el Cliente.
    /// Ruta base: /api/checkout
    /// Rol requerido: Customer.
    /// </summary>
    [Authorize(Roles = AppRoles.Customer)]
    public class CheckoutController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public CheckoutController(ICommandBus bus)
        {
            _bus = bus;
        }

        // ────────────────────────────────────────────────────────────────────
        // POST /api/checkout/create-order-group
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Persiste el carrito en memoria de la App como OrderGroup en estado Draft.
        /// Valida disponibilidad de productos, calcula precios y fees desde AppConfig.
        /// </summary>
        [HttpPost("create-order-group")]
        public async Task<IActionResult> CreateOrderGroup(
            [FromBody] CreateOrderGroupRequest request,
            CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var command = new CreateOrderGroupCommand
            {
                CustomerId = customerId.Value,
                DeliveryAddress = request.DeliveryAddress,
                Lat = request.Lat,
                Lon = request.Lon,
                Notes = request.Notes,
                Merchants = request.Merchants.Select(m => new MerchantCartDto
                {
                    MerchantId = m.MerchantId,
                    Items = m.Items.Select(i => new CartItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        SpecialInstructions = i.SpecialInstructions
                    }).ToList()
                }).ToList()
            };

            var result = await _bus.SendAsync<CreateOrderGroupCommand, CreateOrderGroupResult>(command, ct);

            if (!result.IsSuccess)
                return BadRequest(new { result.ErrorMessage });

            return Ok(new
            {
                result.OrderGroupId,
                result.TotalAmount,
                result.ServiceFee,
                result.DeliveryFee,
                result.SubOrderCount
            });
        }

        // ────────────────────────────────────────────────────────────────────
        // POST /api/checkout/cancel
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Cancela un OrderGroup propio del Customer.
        /// Estados cancelables por Customer: Draft, PendingPayment,
        /// PaymentConfirmed, AwaitingDriverAssignment.
        /// </summary>
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelOrderGroup(
            [FromBody] CancelOrderGroupRequest request,
            CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var command = new CancelOrderGroupCommand
            {
                OrderGroupId = request.OrderGroupId,
                RequestedByUserId = customerId.Value,
                RequestedByRole = AppRoles.Customer,   // siempre Customer en este controller
                CancellationReason = request.CancellationReason
            };

            var result = await _bus.SendAsync<CancelOrderGroupCommand, CancelOrderGroupResult>(command, ct);

            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new
            {
                result.Success,
                result.Message,
                result.DriverReleased,
                result.SubOrdersCancelled
            });
        }

        // ────────────────────────────────────────────────────────────────────
        // POST /api/checkout
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Confirma el carrito y registra el pago.
        /// - Efectivo/Tarjeta → dispatch automático.
        /// - Transferencia    → PendingPayment hasta validación BackOffice.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmCheckout(
            [FromBody] ConfirmCheckoutRequest request,
            CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var command = new ConfirmCheckoutCommand(
                orderGroupId: request.OrderGroupId,
                customerId: customerId.Value,
                paymentMethodId: request.PaymentMethodId,
                amount: request.Amount,
                proofUrl: request.ProofUrl,
                reference: request.Reference);

            var result = await _bus.SendAsync<ConfirmCheckoutCommand, ConfirmCheckoutResult>(command, ct);

            return Ok(new
            {
                result.OrderGroupId,
                result.OrderGroupPaymentId,
                result.OrderGroupStatus,
                result.DispatchStarted,
                result.Message
            });
        }

        // ────
        // GET /api/checkout/payment-methods
        // ────

        /// <summary>
        /// Retorna los métodos de pago activos disponibles para el checkout.
        /// Fuente: cfg.PaymentMethods (IsActive = true).
        /// </summary>
        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods(CancellationToken ct)
        {
            var result = await _bus.QueryAsync<GetActivePaymentMethodsQuery, List<PaymentMethodDto>>(
                new GetActivePaymentMethodsQuery(), ct);

            return Ok(result);
        }

        // ────
        // POST /api/checkout/confirm-order-group
        // ────

        /// <summary>
        /// Consolida la dirección de entrega y recalcula el delivery fee real.
        /// Transiciona el OrderGroup de Draft → CapacityValidated.
        /// Debe llamarse ANTES de POST /api/checkout (pago).
        /// </summary>
        [HttpPost("confirm-order-group")]
        public async Task<IActionResult> ConfirmOrderGroup(
            [FromBody] ConfirmOrderGroupRequest request,
            CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var command = new ConfirmOrderGroupCommand(
                orderGroupId: request.OrderGroupId,
                customerId: customerId.Value,
                shippingAddressId: request.ShippingAddressId);

            var result = await _bus.SendAsync<ConfirmOrderGroupCommand, ConfirmOrderGroupResult>(
                command, ct);

            return Ok(new
            {
                result.OrderGroupId,
                result.Status,
                result.SubTotal,
                result.DeliveryFee,
                result.ServiceFee,
                result.TotalAmount,
                result.DeliveryAddressText
            });
        }
    }
}