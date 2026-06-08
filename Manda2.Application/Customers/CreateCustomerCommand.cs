using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Application.DTOs;
using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manda2.Application.Customers
{
    // Comando (parameters públicos)
    public record CreateCustomerCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string? IdentityDocumentUrl, // URL del archivo subido
        int? CreatedByUserId // puede ser null si es auto-registro
    ) : ICommand;

    // Handler del comando
    public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> HandleAsync(CreateCustomerCommand command, CancellationToken ct = default)
        {
            // Crear entidad
            var customer = new Customer
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                IdentityDocumentUrl = command.IdentityDocumentUrl,
                IsActive = false, // por defecto inactivo hasta validación BackOffice
                CreatedByUserId = command.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(ct); // guardamos para obtener el Id

            // Registrar auditoría (snapshot)
            var audit = new AuditLog
            {
                EntityName = nameof(Customer),
                EntityId = customer.Id,
                Action = "CustomerCreated",
                Data = JsonSerializer.Serialize(new
                {
                    customer.Id,
                    customer.FirstName,
                    customer.LastName,
                    customer.Email,
                    customer.PhoneNumber,
                    customer.IdentityDocumentUrl,
                    customer.CreatedAt
                }),
                PerformedByUserId = command.CreatedByUserId
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
