// PROPÓSITO: Handler de solo lectura para GetDriverProfileQuery.
//            Proyecta Driver a DriverProfileDto.
//            AsNoTracking — solo lectura, máximo performance.
//

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
    /// Handler para <see cref="GetDriverProfileQuery"/>.
    /// </summary>
    public class GetDriverProfileHandler : IQueryHandler<GetDriverProfileQuery, DriverProfileDto>
    {
        private readonly IApplicationDbContext _db;

        public GetDriverProfileHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DriverProfileDto> HandleAsync(
            GetDriverProfileQuery query, CancellationToken ct)
        {
            // ── PASO 1: Proyección directa — sin Include, sin tracking ────
            var dto = await _db.Drivers
                .AsNoTracking()
                .Where(d => d.Id == query.DriverId && !d.IsDeleted)
                .Select(d => new DriverProfileDto
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Email = d.Email,
                    PhoneNumber = d.PhoneNumber,
                    Status = d.Status.ToString(),
                    IsActive = d.IsActive,
                    IsOnline = d.IsOnline,
                    IsCashBlocked = d.IsCashBlocked,
                    CurrentOrderGroupId = d.CurrentOrderGroupId,
                    CurrentCashBalance = d.CurrentCashBalance,
                    MaxCashLimit = d.MaxCashLimit,
                    MaxActiveGroups = d.MaxActiveGroups,
                    MaxSubOrderLimit = d.MaxSubOrderLimit,
                    SapEmployeeCode = d.SAP_EmployeeCode,
                    CreatedAt = d.CreatedAt
                })
                .FirstOrDefaultAsync(ct);

            if (dto == null)
                throw new KeyNotFoundException($"Driver {query.DriverId} no encontrado.");

            return dto;
        }
    }
}
