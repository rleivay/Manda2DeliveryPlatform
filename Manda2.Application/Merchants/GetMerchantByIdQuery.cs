using Manda2.Application.Common;
using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Merchants
{
    public record GetMerchantByIdQuery(int Id) : IQuery<MerchantDto?>;

    public class GetMerchantByIdQueryHandler : IQueryHandler<GetMerchantByIdQuery, MerchantDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetMerchantByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MerchantDto?> HandleAsync(GetMerchantByIdQuery query, CancellationToken ct= default)
        {
            var merchant = await _context.Merchants
                .Where(m => m.Id == query.Id && !m.IsDeleted)
                .Select(m => new MerchantDto(
                    m.Id,
                    m.Name,
                    m.CommercialPhone,
                    m.CommercialEmail,
                    m.IdentityDocumentUrl,
                    m.IsActive,
                    m.Type,
                    m.BankAccountNumber,
                    m.BankAccountType,
                    m.BankName,
                    m.LogoUrl,
                    m.CreatedAt,
                    m.UpdatedAt
                ))
                .FirstOrDefaultAsync(ct);

            return merchant;
        }
    }
}
