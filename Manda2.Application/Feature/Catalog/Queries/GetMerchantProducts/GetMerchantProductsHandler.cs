using Manda2.Application.Common;
using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Manda2.Contracts.Catalog;

namespace Manda2.Application.Feature.Catalog.Queries.GetMerchantProducts
{
    public class GetMerchantProductsHandler
        : IQueryHandler<GetMerchantProductsQuery, List<MerchantProductDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetMerchantProductsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<MerchantProductDto>> HandleAsync(
            GetMerchantProductsQuery query, CancellationToken ct)
        {
            return await _db.MerchantProducts
                .Where(mp => mp.MerchantId == query.MerchantId && mp.IsAvailable)
                .Include(mp => mp.Product)
                    .ThenInclude(p => p.Category)
                .Select(mp => new MerchantProductDto
                {
                    MerchantProductId = mp.Id,
                    ProductId = mp.ProductId,
                    MerchantId = mp.MerchantId,
                    Name = mp.Product.Name,
                    Description = mp.Product.Description,
                    CategoryName = mp.Product.Category.Name,
                    ImageUrl = mp.Product.ImageUrl,
                    BasePrice = mp.BasePrice,
                    SalePrice = mp.SalePrice,
                    ShowSamePriceLabel = mp.ShowSamePriceLabel,
                    IsAvailable = mp.IsAvailable,
                    CommissionSource = mp.CommissionSource.ToString(),
                    ResolvedCommissionPct = mp.ResolvedCommissionPct
                })
                .ToListAsync(ct);
        }
    }
}
