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
    public record CreateDriverCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string IdentityDocumentUrl,
    decimal MaxCashLimit,
    int MaxActiveGroups,
    string? BankAccountNumber,
    string? BankAccountType,
    string? BankName,
    int CreatedByUserId
) : ICommand;

    public class CreateDriverCommandHandler : ICommandHandler<CreateDriverCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateDriverCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> HandleAsync(CreateDriverCommand command, CancellationToken ct = default)
        {
            var driver = new Driver
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                IdentityDocumentUrl = command.IdentityDocumentUrl,
                MaxCashLimit = command.MaxCashLimit,
                MaxActiveGroups = command.MaxActiveGroups,
                BankAccountNumber = command.BankAccountNumber,
                BankAccountType = command.BankAccountType,
                BankName = command.BankName,
                IsActive = true,
                CurrentCashBalance = 0,
                IsCashBlocked = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = command.CreatedByUserId
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
