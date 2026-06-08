using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Manda2.Application.Merchants
{
    public record CreateMerchantCommand(
        string Name,
        string CommercialPhone,
        string CommercialEmail,
        MerchantType Type,
        string? IdentityDocumentUrl,
        string? BankAccountNumber,
        string? BankAccountType,
        string? BankName,
        string AddressText,
        int? CreatedByUserId,
        string logoUrl
    ) : ICommand;

    public class CreateMerchantCommandHandler : ICommandHandler<CreateMerchantCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateMerchantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> HandleAsync(CreateMerchantCommand command, CancellationToken ct=default)
        {
            var merchant = new Merchant
            {
                Name = command.Name,
                CommercialPhone = command.CommercialPhone,
                CommercialEmail = command.CommercialEmail,
                Type = command.Type,
                IdentityDocumentUrl = command.IdentityDocumentUrl,
                BankAccountNumber = command.BankAccountNumber,
                BankAccountType = command.BankAccountType,
                BankName = command.BankName,
                AddressText=command.AddressText,
                LogoUrl=command.logoUrl,
                IsActive = false, // BackOffice validation required
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = command.CreatedByUserId
            };

            _context.Merchants.Add(merchant);
            await _context.SaveChangesAsync(ct);

            var audit = new AuditLog
            {
                EntityName = nameof(Merchant),
                EntityId = merchant.Id,
                Action = "MerchantCreated",
                Data = JsonSerializer.Serialize(new
                {
                    merchant.Id,
                    merchant.Name,
                    merchant.CommercialEmail,
                    merchant.Type,
                    merchant.CreatedAt
                }),
                PerformedByUserId = command.CreatedByUserId
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
