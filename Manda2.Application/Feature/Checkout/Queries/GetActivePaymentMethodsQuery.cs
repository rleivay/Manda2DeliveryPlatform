using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Checkout.Queries
{
    /// <summary>
    /// Retorna los métodos de pago activos del catálogo cfg.PaymentMethods.
    /// Sin parámetros — catálogo global.
    /// </summary>
    public record GetActivePaymentMethodsQuery() : IQuery<List<PaymentMethodDto>>;

    /// <summary>
    /// Handler de solo lectura. AsNoTracking — catálogo estático.
    /// </summary>
    public class GetActivePaymentMethodsHandler
        : IQueryHandler<GetActivePaymentMethodsQuery, List<PaymentMethodDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetActivePaymentMethodsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PaymentMethodDto>> HandleAsync(
            GetActivePaymentMethodsQuery query,
            CancellationToken ct)
        {
            return await _db.PaymentMethods
                .AsNoTracking()
                .Where(pm => pm.IsActive && !pm.IsDeleted)
                .OrderBy(pm => pm.Id)
                .Select(pm => new PaymentMethodDto
                {
                    Id = pm.Id,
                    Code = pm.Code,
                    Name = pm.Name,
                    RequiresReference = pm.RequiresReference,
                    RequiresVoucher = pm.RequiresVoucher
                })
                .ToListAsync(ct);
        }
    }
}
