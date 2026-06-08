using Manda2.Application.Common;
using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Manda2.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Queries.GetMerchantsByCategory
{
    /// <summary>
    /// Retorna comercios aprobados, opcionalmente filtrados por categoría.
    /// Proyecta AddressText, Latitude y Longitude para el header del detalle.
    /// </summary>
    public class GetMerchantsByCategoryHandler
        : IQueryHandler<GetMerchantsByCategoryQuery, List<MerchantSummaryDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetMerchantsByCategoryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<MerchantSummaryDto>> HandleAsync(
            GetMerchantsByCategoryQuery query, CancellationToken ct)
        {
            var q = _db.Merchants
                .Where(m => !m.IsDeleted && m.IsApproved)
                .Include(m => m.Category)
                .AsQueryable();

            if (query.CategoryId.HasValue)
                q = q.Where(m => m.MerchantCategoryId == query.CategoryId.Value);

            return await q.Select(m => new MerchantSummaryDto
            {
                MerchantId = m.Id,
                Name = m.Name,
                CategoryName = m.Category != null ? m.Category.Name : null,
                CategoryId = m.MerchantCategoryId,
                CommissionPct = m.CommissionPct,
                IsOpen = m.IsOnline,
                LogoUrl = m.LogoUrl,
                // ── v2: ubicación ──────────────────────────────────────────────
                AddressText = m.AddressText,
                Latitude = m.Latitude,
                Longitude = m.Longitude
            })
            .ToListAsync(ct);
        }
    }
}
