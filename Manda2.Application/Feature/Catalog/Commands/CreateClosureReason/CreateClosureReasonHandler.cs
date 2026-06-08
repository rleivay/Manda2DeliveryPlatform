using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateClosureReason
{
    public class CreateClosureReasonHandler : ICommandHandler<CreateClosureReasonCommand, int>
    {
        private readonly IApplicationDbContext _db;
        public CreateClosureReasonHandler(IApplicationDbContext db) => _db = db;

        public async Task<int> HandleAsync(CreateClosureReasonCommand command, CancellationToken ct)
        {
            var reason = new ClosureReason { Name = command.Name, Description = command.Description, IsActive = true };
            _db.ClosureReasons.Add(reason);
            await _db.SaveChangesAsync(ct);
            return reason.Id;
        }
    }
}
