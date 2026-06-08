using Manda2.Application.Common;
using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Drivers
{
    public record GetDriverByIdQuery(int Id) : IQuery<DriverDto?>;

    public class GetDriverByIdQueryHandler : IQueryHandler<GetDriverByIdQuery, DriverDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetDriverByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DriverDto?> HandleAsync(GetDriverByIdQuery query, CancellationToken ct = default)
        {
            var driver = await _context.Drivers
                .Where(d => d.Id == query.Id && !d.IsDeleted)
                .Select(d => new DriverDto(
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.Email,
                    d.PhoneNumber,
                    d.IdentityDocumentUrl,
                    d.IsActive,
                    d.MaxCashLimit,
                    d.CurrentCashBalance,
                    d.IsCashBlocked,
                    d.MaxActiveGroups,
                    d.BankAccountNumber,
                    d.BankAccountType,
                    d.BankName,
                    d.CreatedAt,
                    d.UpdatedAt
                ))
                .FirstOrDefaultAsync(ct);

            return driver;
        }
    }
}
