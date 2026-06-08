using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Merchants
{
    public record ApproveMerchantCommand(int MerchantId, int ApprovedByUserId) : ICommand;

    public class ApproveMerchantCommandHandler : ICommandHandler<ApproveMerchantCommand>
    {
        private readonly IApplicationDbContext _context;

        public ApproveMerchantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> HandleAsync(ApproveMerchantCommand cmd, CancellationToken ct= default)
        {
            var merchant = await _context.Merchants.FindAsync(cmd.MerchantId);
            if (merchant == null) throw new Exception("Merchant no encontrado.");

            merchant.IsActive = true;
            merchant.UpdatedAt = DateTime.UtcNow;
            merchant.UpdatedByUserId = cmd.ApprovedByUserId;

            _context.Merchants.Update(merchant);
            await _context.SaveChangesAsync(ct);

            // Registrar auditoría
            var audit = new AuditLog
            {
                EntityName = nameof(Merchant),
                EntityId = merchant.Id,
                Action = "MerchantApproved",
                Data = $"Merchant aprobado por usuario {cmd.ApprovedByUserId}",
                PerformedByUserId = cmd.ApprovedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
