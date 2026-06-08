using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateScheduleException
{
    public record CreateScheduleExceptionCommand(
    ActorType ActorType,
    int ActorId,
    DateOnly Date,
    bool IsClosed,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    int? ClosureReasonId,
    string? Notes) : ICommand<int>;
}
