using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace Manda2.Application.Actors.Commands
{
    public record UploadIdentityDocumentCommand(
        int ActorId,
        string ActorType,
        IFormFile Document
    ) : ICommand;

    public class UploadIdentityDocumentCommandHandler : ICommandHandler<UploadIdentityDocumentCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostEnvironment _env;

        public UploadIdentityDocumentCommandHandler(
            IApplicationDbContext context,
            IHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<Unit> HandleAsync(UploadIdentityDocumentCommand cmd, CancellationToken ct=default)
        {
            string folderPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "identity");
            Directory.CreateDirectory(folderPath);

            string fileName = $"{cmd.ActorType}_{cmd.ActorId}_{Guid.NewGuid()}{Path.GetExtension(cmd.Document.FileName)}";
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = File.Create(filePath))
            {
                await cmd.Document.CopyToAsync(stream, ct);
            }

            string url = $"/uploads/identity/{fileName}";

            // Actualizar entidad según tipo
            switch (cmd.ActorType.ToLower())
            {
                case "0":
                    var customer = await _context.Customers.FindAsync(cmd.ActorId);
                    if (customer != null)
                    {
                        customer.IdentityDocumentUrl = url;
                        _context.Customers.Update(customer);
                    }
                    break;

                case "1":
                    var merchant = await _context.Merchants.FindAsync(cmd.ActorId);
                    if (merchant != null)
                    {
                        merchant.IdentityDocumentUrl = url;
                        _context.Merchants.Update(merchant);
                    }
                    break;

                case "2":
                    var driver = await _context.Drivers.FindAsync(cmd.ActorId);
                    if (driver != null)
                    {
                        driver.IdentityDocumentUrl = url;
                        _context.Drivers.Update(driver);
                    }
                    break;
            }

            await _context.SaveChangesAsync(ct);

            // Registrar auditoría
            var audit = new AuditLog
            {
                EntityName = cmd.ActorType,
                EntityId = cmd.ActorId,
                Action = "DocumentUploaded",
                Data = $"Documento cargado: {url}",
                PerformedByUserId = null, // Si viene de front, puedes pasar UserId como parámetro
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
