using Manda2.Application.Common;
using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Actors.Queries
{
    public record DownloadIdentityDocumentQuery(int ActorId, string ActorType) : IQuery<DownloadDocumentResponse>;

    public class DownloadIdentityDocumentQueryHandler : IQueryHandler<DownloadIdentityDocumentQuery, DownloadDocumentResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostEnvironment _env;

        public DownloadIdentityDocumentQueryHandler(
            IApplicationDbContext context,
            IHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<DownloadDocumentResponse> HandleAsync(DownloadIdentityDocumentQuery query, CancellationToken ct=default)
        {
            string? documentUrl = query.ActorType.ToLower() switch
            {
                "customer" => (await _context.Customers.FindAsync(query.ActorId))?.IdentityDocumentUrl,
                "merchant" => (await _context.Merchants.FindAsync(query.ActorId))?.IdentityDocumentUrl,
                "driver" => (await _context.Drivers.FindAsync(query.ActorId))?.IdentityDocumentUrl,
                _ => throw new ArgumentException("Tipo de actor inválido")
            };

            if (string.IsNullOrEmpty(documentUrl))
                throw new Exception("Documento no encontrado.");

            string fullPath = Path.Combine(_env.ContentRootPath, "wwwroot", documentUrl.TrimStart('/'));
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Archivo no encontrado en el servidor.");

            byte[] bytes = await File.ReadAllBytesAsync(fullPath, ct);
            string contentType = GetContentType(fullPath);
            string fileName = Path.GetFileName(fullPath);

            return new DownloadDocumentResponse(fileName, bytes, contentType);
        }

        private static string GetContentType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
