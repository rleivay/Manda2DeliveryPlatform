using Manda2.Application.Feature.Config.Queries;
using Manda2.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class ConfigController : BaseApiController
    {
        private readonly ICommandBus _bus;
        public ConfigController(ICommandBus bus) => _bus = bus;

        [HttpGet("{key}")]
        [AllowAnonymous] // Requerido para que la app lo lea incluso antes del login completo si es necesario
        public async Task<IActionResult> GetConfig(string key, CancellationToken ct)
        {
            var value = await _bus.QueryAsync<GetConfigValueQuery, string?>(new GetConfigValueQuery(key), ct);
            return Ok(new { Key = key, Value = value });
        }
    }
}
