using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateScheduleException
{
    public class CreateScheduleExceptionHandler : ICommandHandler<CreateScheduleExceptionCommand, int>
    {
        private readonly IApplicationDbContext _db;
        public CreateScheduleExceptionHandler(IApplicationDbContext db) => _db = db;

        public async Task<int> HandleAsync(CreateScheduleExceptionCommand command, CancellationToken ct)
        {
            var exception = new OperatingScheduleException
            {
                ActorType = command.ActorType,
                MerchantId = command.ActorType == ActorType.Merchant ? command.ActorId : null,
                DriverId = command.ActorType == ActorType.Driver ? command.ActorId : null,
                Date = command.Date,
                IsClosed = command.IsClosed,
                OpenTime = command.OpenTime,
                CloseTime = command.CloseTime,
                ClosureReasonId = command.ClosureReasonId,
                Notes = command.Notes
            };

            _db.OperatingScheduleExceptions.Add(exception);
            await _db.SaveChangesAsync(ct);
            return exception.Id;
        }
    }
}
