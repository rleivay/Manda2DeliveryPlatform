using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Config.Queries
{
    public class GetConfigValueQueryHandler : IQueryHandler<GetConfigValueQuery, string?>
    {
        private readonly IApplicationDbContext _db;
        public GetConfigValueQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<string?> HandleAsync(GetConfigValueQuery query, CancellationToken ct)
        {
            return await _db.AppConfigs
                .AsNoTracking()
                .Where(x => x.Key == query.Key && !x.IsDeleted)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(ct);
        }
    }
}
