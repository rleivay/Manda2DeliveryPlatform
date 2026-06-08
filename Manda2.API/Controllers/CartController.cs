using Manda2.API.Extensions;
using Manda2.Application.Feature.Cart.Commands;
using Manda2.Application.Feature.Cart.Queries;
using Manda2.Application.Mediator;
using Manda2.Contracts.Cart;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// Carrito multi-comercio. Representa el OrderGroup en estado Draft.
    /// Ruta base: /api/cart
    /// </summary>
    [Authorize(Roles = AppRoles.Customer)]
    public class CartController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public CartController(ICommandBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// GET /api/cart/{orderGroupId}
        /// Retorna el carrito consolidado multi-comercio.
        /// </summary>
        [HttpGet("{orderGroupId:int}")]
        public async Task<IActionResult> GetCart(int orderGroupId, CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var result = await _bus.QueryAsync<GetCartQuery, CartDto>(
                new GetCartQuery(orderGroupId, customerId.Value), ct);

            return Ok(result);
        }

        /// <summary>
        /// PATCH /api/cart/items/{detailId}/quantity
        /// Actualiza la cantidad de un item y recalcula totales en cascada.
        /// </summary>
        [HttpPatch("items/{detailId:int}/quantity")]
        public async Task<IActionResult> UpdateQuantity(
            int detailId,
            [FromBody] UpdateQuantityRequest request,
            CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var result = await _bus.SendAsync<UpdateCartItemQuantityCommand, UpdateCartItemQuantityResult>(
                new UpdateCartItemQuantityCommand(detailId, request.NewQuantity, customerId.Value), ct);

            return Ok(result);
        }

        /// <summary>
        /// DELETE /api/cart/items/{detailId}
        /// Elimina un item. Si la SubOrder queda vacía → la elimina.
        /// Si el OrderGroup queda vacío → lo elimina.
        /// </summary>
        [HttpDelete("items/{detailId:int}")]
        public async Task<IActionResult> RemoveItem(int detailId, CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var result = await _bus.SendAsync<RemoveCartItemCommand, RemoveCartItemResult>(
                new RemoveCartItemCommand(detailId, customerId.Value), ct);

            return Ok(result);
        }
    }

    /// <summary>DTO de entrada para PATCH quantity.</summary>
    public record UpdateQuantityRequest(int NewQuantity);
}
