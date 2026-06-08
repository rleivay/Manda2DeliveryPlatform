// BaseApiController.cs
//
// Objetivo:
// - Centralizar respuestas comunes (Ok, NotFound, etc.)
// - Evitar repetir código en cada controlador
// - Establecer ruta base común

using Microsoft.AspNetCore.Mvc;

namespace Manda2.API.Controllers
{
    /// <summary>
    /// Controlador base para todos los controladores de la API.
    /// Proporciona métodos auxiliares comunes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Todos los controladores responderán bajo /api/{nombre}
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Devuelve Ok si hay datos, o NotFound si es null.
        /// </summary>
        /// <typeparam name="T">Tipo de dato</typeparam>
        /// <param name="data">Resultado de una consulta</param>
        /// <returns>OkObjectResult o NotFoundResult</returns>
        protected IActionResult OkOrNotFound<T>(T data)
        {
            return data == null ? NotFound() : Ok(data);
        }
    }
}
