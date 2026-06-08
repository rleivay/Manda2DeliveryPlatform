using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Queries.GetMerchantCategories
{
    /// <summary>
    /// Handler para <see cref="GetMerchantCategoriesQuery"/>.
    /// </summary>
    public class GetMerchantCategoriesHandler
        : IQueryHandler<GetMerchantCategoriesQuery, List<MerchantCategoryDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetMerchantCategoriesHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<MerchantCategoryDto>> HandleAsync(
            GetMerchantCategoriesQuery query, CancellationToken ct)
        {
            return await _db.MerchantCategories
                .AsNoTracking()
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new MerchantCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IconUrl = c.IconUrl
                })
                .ToListAsync(ct);
        }
    }
}
