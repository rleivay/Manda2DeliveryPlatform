using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Customers
{
    public record ApproveCustomerCommand(int CustomerId, int ApprovedByUserId) : ICommand;

    public class ApproveCustomerCommandHandler : ICommandHandler<ApproveCustomerCommand>
    {
        private readonly IApplicationDbContext _context;

        public ApproveCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> HandleAsync(ApproveCustomerCommand cmd, CancellationToken ct=default)
        {
            var customer = await _context.Customers.FindAsync(cmd.CustomerId);
            if (customer == null) throw new Exception("Customer no encontrado.");

            customer.IsActive = true;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedByUserId = cmd.ApprovedByUserId;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync(ct);

            var audit = new AuditLog
            {
                EntityName = nameof(Customer),
                EntityId = customer.Id,
                Action = "CustomerApproved",
                Data = $"Cliente aprobado por usuario {cmd.ApprovedByUserId}",
                PerformedByUserId = cmd.ApprovedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
