using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Manda2.Application.Customers
{
    public record GetCustomerByIdQuery(int Id) : IQuery<CustomerDto?>;

    public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto?> HandleAsync(GetCustomerByIdQuery query, CancellationToken ct = default)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == query.Id && !c.IsDeleted)
                .Select(c => new CustomerDto(
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.PhoneNumber,
                    c.IdentityDocumentUrl,
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt
                ))
                .FirstOrDefaultAsync(ct);

            return customer;
        }
    }
}
