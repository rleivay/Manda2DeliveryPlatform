using Manda2.Application.Actors.Commands;
using Manda2.Application.Actors.Queries;
using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Manda2.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Manda2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DocumentsController : BaseApiController
    {
        private readonly ICommandBus _bus;

        public DocumentsController(ICommandBus bus)
        {
            _bus = bus;
        }

        /// <summary>
        /// POST api/documents/identity
        /// Cualquier actor autenticado puede subir su propio documento.
        /// El handler valida que el actorId corresponda al actor del JWT.
        /// </summary>
        [HttpPost("identity")]
        public async Task<IActionResult> UploadIdentityDocument(
            [FromForm] UploadIdentityDocumentRequest request, CancellationToken ct)
        {
            if (request.Document is null || request.Document.Length == 0)
                return BadRequest("Debe adjuntar un documento válido.");

            var cmd = new UploadIdentityDocumentCommand(
                request.ActorId,
                request.ActorType,
                request.Document);

            await _bus.SendAsync<UploadIdentityDocumentCommand, Unit>(cmd, ct);
            return Ok(new { Message = "Documento subido exitosamente" });
        }

        /// <summary>
        /// GET api/documents/identity?actorId=1&actorType=Driver
        /// Solo BackOffice/Admin puede descargar documentos de identidad.
        /// </summary>
        [HttpGet("identity")]
       // [Authorize(Roles = $"{AppRoles.BackOffice},{AppRoles.Admin}")]
        public async Task<IActionResult> DownloadIdentityDocument(
            [FromQuery] int actorId,
            [FromQuery] string actorType,
            CancellationToken ct)
        {
            var query = new DownloadIdentityDocumentQuery(actorId, actorType);
            var result = await _bus.QueryAsync<DownloadIdentityDocumentQuery, DownloadDocumentResponse>(query, ct);

            if (result is null) return NotFound("Documento no encontrado.");

            return File(result.Content, result.ContentType, result.FileName);
        }
    }
}
