using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Drivers
{
    public record ApproveDriverCommand(int DriverId, int ApprovedByUserId) : ICommand;

    public class ApproveDriverCommandHandler : ICommandHandler<ApproveDriverCommand>
    {
        private readonly IApplicationDbContext _context;

        public ApproveDriverCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> HandleAsync(ApproveDriverCommand cmd, CancellationToken ct = default)
        {
            var driver = await _context.Drivers.FindAsync(cmd.DriverId);
            if (driver == null) throw new Exception("Driver no encontrado.");

            driver.IsActive = true;
            driver.UpdatedAt = DateTime.UtcNow;
            driver.UpdatedByUserId = cmd.ApprovedByUserId;

            _context.Drivers.Update(driver);
            await _context.SaveChangesAsync(ct);

            var audit = new AuditLog
            {
                EntityName = nameof(Driver),
                EntityId = driver.Id,
                Action = "DriverApproved",
                Data = $"Repartidor aprobado por usuario {cmd.ApprovedByUserId}",
                PerformedByUserId = cmd.ApprovedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
