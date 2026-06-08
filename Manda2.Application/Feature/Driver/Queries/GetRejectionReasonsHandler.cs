// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetRejectionReasons/
//          GetRejectionReasonsHandler.cs
//
// PROPÓSITO: Retorna el catálogo de motivos de rechazo activos ordenados
//            por DisplayOrder para poblar el Picker en la app MAUI.
// ═══════════════════════════════════════════════════════════════════════════


using Manda2.Application.Common;
using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Queries
{
    /// <summary>
    /// Handler de GetRejectionReasonsQuery.
    /// </summary>
    public class GetRejectionReasonsHandler
        : IQueryHandler<GetRejectionReasonsQuery, List<RejectionReasonDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetRejectionReasonsHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<RejectionReasonDto>> HandleAsync(
            GetRejectionReasonsQuery query, CancellationToken ct)
        {
            // Solo retorna motivos activos, ordenados para la UI.
            // AsNoTracking: operación de solo lectura, máxima performance.
            return await _db.DriverRejectionReasons
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RejectionReasonDto
                {
                    Id = r.Id,
                    DisplayName = r.DisplayName,
                    RequiresNote = r.RequiresNote
                })
                .ToListAsync(ct);
        }
    }
}
