using Manda2.API.Extensions;
using Manda2.Application.Customers;
using Manda2.Application.DTOs;
using Manda2.Application.Feature.Customer.Commands;
using Manda2.Application.Feature.Customer.Queries;
using Manda2.Application.Mediator;
using Manda2.Contracts.Customer;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    [Authorize]
    public class CustomersController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public CustomersController(ICommandBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// GET api/customers/{id}
        /// Customer: solo puede consultar su propio perfil (validado contra JWT).
        /// BackOffice/Admin: puede consultar cualquier perfil.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{AppRoles.Customer},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            // ── Seguridad: Customer solo ve su propio perfil ──────────────
            if (User.IsInRole(AppRoles.Customer))
            {
                int? customerId = User.GetCustomerId();
                if (customerId == null || customerId.Value != id)
                    return Forbid();
            }

            var result = await _bus.QueryAsync<GetCustomerByIdQuery, CustomerDto?>(
                new GetCustomerByIdQuery(id), ct);
            return OkOrNotFound(result);
        }

        /// <summary>
        /// POST api/customers
        /// Registro público — no requiere token.
        /// (En el flujo normal se usa api/auth/register; este endpoint
        ///  queda para creación directa desde BackOffice si se requiere.)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(
            [FromBody] CreateCustomerCommand command, CancellationToken ct)
        {
            await _bus.SendAsync<CreateCustomerCommand, Unit>(command, ct);
            return Ok(new { Message = "Customer creado (pendiente validación BackOffice)" });
        }

        /// <summary>
        /// PATCH api/customers/{id}/approve
        /// Solo BackOffice/Admin puede aprobar clientes.
        /// </summary>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> Approve(
            int id, [FromBody] ApproveCustomerCommand cmd, CancellationToken ct)
        {
            if (id != cmd.CustomerId) return BadRequest("IDs no coinciden.");
            await _bus.SendAsync<ApproveCustomerCommand, Unit>(cmd, ct);
            return Ok(new { Message = "Cliente aprobado exitosamente" });
        }

        /// <summary>
        /// GET api/customers/me/shipping-addresses
        /// Retorna las direcciones de envío activas del cliente autenticado.
        /// Usado en el flujo de checkout para selección de dirección de entrega.
        /// </summary>
        [HttpGet("me/shipping-addresses")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> GetMyShippingAddresses(CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            var result = await _bus.QueryAsync<GetCustomerShippingAddressesQuery, List<ShippingAddressDto>>(
                new GetCustomerShippingAddressesQuery(customerId.Value), ct);

            return Ok(result);
        }

        [HttpPost("me/shipping-addresses")]
        [Authorize(Roles = AppRoles.Customer)]
        public async Task<IActionResult> CreateMyShippingAddress(
            [FromBody] CreateShippingAddressRequest request,
            CancellationToken ct)
        {
            int? customerId = User.GetCustomerId();
            if (customerId == null)
                return Unauthorized("Token no contiene un CustomerId válido.");

            int userId = User.GetUserId();

            var command = new CreateShippingAddressCommand
            {
                CustomerId = customerId.Value,
                CreatedByUserId = userId > 0 ? userId : null,
                Alias = request.Alias,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsDefault = request.IsDefault,
                ContactPhone = request.ContactPhone,
                PhoneCountryCode = request.PhoneCountryCode,
                ApartmentNumber = request.ApartmentNumber,
                DeliveryNotes = request.DeliveryNotes
            };

            var id = await _bus.SendAsync<CreateShippingAddressCommand, int>(command, ct);

            return Ok(id);
        }
    }
}