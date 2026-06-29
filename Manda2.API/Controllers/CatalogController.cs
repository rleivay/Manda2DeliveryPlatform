using Manda2.Application.Common;
using Manda2.Application.Feature.Catalog.Commands.CreateClosureReason;
using Manda2.Application.Feature.Catalog.Commands.CreateMerchantCategory;
using Manda2.Application.Feature.Catalog.Commands.CreateMerchantProduct;
using Manda2.Application.Feature.Catalog.Commands.CreateProductCategory;
using Manda2.Application.Feature.Catalog.Commands.CreateScheduleException;
using Manda2.Application.Feature.Catalog.Commands.SeedCatalog;
using Manda2.Application.Feature.Catalog.Commands.UpdateOperatingSchedule;
using Manda2.Application.Feature.Catalog.Queries.GetMerchantCategories;
using Manda2.Application.Feature.Catalog.Queries.GetMerchantProducts;
using Manda2.Application.Feature.Catalog.Queries.GetMerchantsByCategory;
using Manda2.Application.Mediator;
using Manda2.Application.Services;
using Manda2.Contracts.Catalog;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;

namespace Manda2.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly ICommandBus _bus;

        public CatalogController(ICommandBus bus)
        {
            _bus = bus;
        }

        // ─── ENDPOINTS PÚBLICOS (App Cliente) ─────────────────────────────

        /// <summary>
        /// GET api/v1/catalog/merchants?categoryId=1
        /// Público — catálogo de comercios activos para la app cliente.
        /// </summary>
        [HttpGet("merchants")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MerchantSummaryDto>>> GetMerchants(
            [FromQuery] int? categoryId, CancellationToken ct)
        {
            var result = await _bus.QueryAsync<GetMerchantsByCategoryQuery, List<MerchantSummaryDto>>(
                new GetMerchantsByCategoryQuery(categoryId), ct);
            return Ok(result);
        }

        /// <summary>
        /// GET api/v1/catalog/merchant-categories
        /// Público — catálogo de categorías de comercio para la app cliente.
        /// Construye IconUrl absoluta usando el host de la request,
        /// igual que GetOpenMerchants → para que la app Mobile cargue la imagen directamente.
        /// </summary>
        [HttpGet("merchant-categories")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MerchantCategoryDto>>> GetMerchantCategories(
            [FromServices] IHttpContextAccessor httpContextAccessor,
            [FromServices] IApplicationDbContext db,
            CancellationToken ct)
        {
            // Construye base URL dinámica: http://192.168.100.67:5068
            var request = httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null
                ? $"{request.Scheme}://{request.Host}"
                : string.Empty;

            var categories = await db.MerchantCategories
                .AsNoTracking()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new MerchantCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IconUrl = c.IconUrl != null && c.IconUrl.StartsWith("/")
                        ? baseUrl + c.IconUrl
                        : c.IconUrl
                })
                .ToListAsync(ct);

            return Ok(categories);
        }

        /// <summary>
        /// GET api/v1/catalog/products/{merchantId}
        /// Público — productos de un comercio para la app cliente.
        /// </summary>
        [HttpGet("products/{merchantId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MerchantProductDto>>> GetProducts(
            int merchantId, CancellationToken ct)
        {
            var result = await _bus.
                QueryAsync<GetMerchantProductsQuery, List<MerchantProductDto>>(
                new GetMerchantProductsQuery(merchantId), ct);
            return Ok(result);
        }

        /// <summary>
        /// GET api/v1/catalog/merchants/{id}/status
        /// Público — disponibilidad actual del comercio.
        /// </summary>
        [HttpGet("merchants/{id}/status")]
        [AllowAnonymous]
        public async Task<ActionResult> GetMerchantStatus(
            int id,
            [FromServices] AvailabilityService service,
            [FromServices] IApplicationDbContext db)
        {
            var m = await db.Merchants
                .Include(x => x.OperatingSchedules)
                .Include(x => x.OperatingScheduleExceptions)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (m == null) return NotFound();

            return Ok(new
            {
                IsEffectivelyAvailable = service.IsCurrentlyAvailable(m),
                IsOnlineFlag = m.IsOnline,
                IsActiveFlag = m.IsActive
            });
        }

        /// <summary>
        /// GET api/v1/catalog/merchants/open
        /// Público — lista de comercios abiertos en este momento.
        /// </summary>
        /// Construye LogoUrl absoluta usando el host de la request,
        /// para que la app Mobile pueda cargar la imagen directamente.
        /// </summary>
        [HttpGet("merchants/open")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MerchantSummaryDto>>> GetOpenMerchants(
            [FromServices] AvailabilityService service,
            [FromServices] IApplicationDbContext db,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            // Construye base URL dinámica: http://192.168.100.67:5068
            var request = httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null
                ? $"{request.Scheme}://{request.Host}"
                : string.Empty;

            var expression = service.IsAvailableExpression();
            var openMerchants = await db.Merchants
                .Where(expression)
                .Include(m => m.Category)
                .Select(m => new MerchantSummaryDto
                {
                    MerchantId = m.Id,
                    Name = m.Name,
                    CategoryName = m.Category != null ? m.Category.Name : null,
                    CategoryId = m.MerchantCategoryId,
                    CommissionPct = m.CommissionPct,
                    IsOpen = m.IsOnline,
                    // Concatena baseUrl solo si LogoUrl es una ruta relativa
                    LogoUrl = m.LogoUrl != null && m.LogoUrl.StartsWith("/")
                                        ? baseUrl + m.LogoUrl
                                        : m.LogoUrl,
                                        // ── v2: ubicación ──────────────────────────────────────────
                AddressText = m.AddressText,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude
                })
                .ToListAsync(ct);

            return Ok(openMerchants);
        }

        // ─── ENDPOINTS MERCHANT / BACKOFFICE ──────────────────────────────

        /// <summary>
        /// POST api/v1/catalog/merchant-product
        /// Registra un producto en un comercio.
        /// Merchant solo puede registrar productos en su propio comercio
        /// (validación en handler contra JWT MerchantId).
        /// </summary>
        [HttpPost("merchant-product")]
        //[Authorize(Roles = $"{AppRoles.Merchant},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult<CreateMerchantProductResult>> CreateMerchantProduct(
            [FromBody] CreateMerchantProductCommand command, CancellationToken ct)
        {
            if (command.MerchantId <= 0 || command.ProductId <= 0)
                return BadRequest("ID de comercio o producto no válido.");

            var result = await _bus.SendAsync<CreateMerchantProductCommand, CreateMerchantProductResult>(command, ct);
            return result.IsAvailable ? CreatedAtAction(null, result) : Ok(result);
        }

        /// <summary>
        /// PUT api/v1/catalog/operating-schedule
        /// Actualiza el horario de un comercio.
        /// </summary>
        [HttpPut("operating-schedule")]
        //[Authorize(Roles = $"{AppRoles.Merchant},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult<string>> UpdateOperatingSchedule(
            [FromBody] UpdateOperatingScheduleCommand command, CancellationToken ct)
        {
            var result = await _bus.SendAsync<UpdateOperatingScheduleCommand, string>(command, ct);
            return Ok(new { message = result });
        }

        /// <summary>
        /// POST api/v1/catalog/schedule-exceptions
        /// Registra una excepción de horario (cierre especial, horario extendido).
        /// </summary>
        [HttpPost("schedule-exceptions")]
        [Authorize(Roles = $"{AppRoles.Merchant},{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult<int>> CreateScheduleException(
            [FromBody] CreateScheduleExceptionCommand command)
            => Ok(await _bus.SendAsync<CreateScheduleExceptionCommand, int>(command));

        // ─── ENDPOINTS SOLO BACKOFFICE / ADMIN ────────────────────────────

        /// <summary>POST api/v1/catalog/merchant-categories</summary>
        [HttpPost("merchant-categories")]
        [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult<int>> CreateMerchantCategory(
            [FromBody] CreateMerchantCategoryCommand command, CancellationToken ct)
        {
            var id = await _bus.SendAsync<CreateMerchantCategoryCommand, int>(command, ct);
            return CreatedAtAction(null, new { id });
        }

        /// <summary>POST api/v1/catalog/product-categories</summary>
        [HttpPost("product-categories")]
        //[Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult<int>> CreateProductCategory(
            [FromBody] CreateProductCategoryCommand command, CancellationToken ct)
        {
            var id = await _bus.SendAsync<CreateProductCategoryCommand, int>(command, ct);
            return CreatedAtAction(null, new { id });
        }

        /// <summary>POST api/v1/catalog/closure-reasons</summary>
        [HttpPost("closure-reasons")]
        //[Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult<int>> CreateClosureReason(
            [FromBody] CreateClosureReasonCommand command)
            => Ok(await _bus.SendAsync<CreateClosureReasonCommand, int>(command));

        /// <summary>
        /// GET api/v1/catalog/drivers/open
        /// Dato interno — solo BackOffice/Admin.
        /// </summary>
        [HttpGet("drivers/open")]
        //[Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<ActionResult> GetOpenDrivers(
            [FromServices] AvailabilityService service,
            [FromServices] IApplicationDbContext db,
            CancellationToken ct)
        {
            var expression = service.IsDriverAvailableExpression();
            var drivers = await db.Drivers
                .Where(expression)
                .Select(d => new { d.Id, d.FirstName, d.LastName, d.MaxActiveGroups })
                .ToListAsync(ct);
            return Ok(drivers);
        }

        // ─── SOLO ADMIN ───────────────────────────────────────────────────

        /// <summary>POST api/v1/catalog/internal/seed — Solo Admin.</summary>
        [HttpPost("internal/seed")]
        //[Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<string>> RunSeed(CancellationToken ct)
        {
            var result = await _bus.SendAsync<SeedCatalogCommand, string>(new SeedCatalogCommand(), ct);
            return Ok(result);
        }
    }
}
