using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.UpdateOperatingSchedule
{
    public record UpdateOperatingScheduleCommand(
        ActorType ActorType,
        int ActorId,
        List<ScheduleDayDto> Days) : ICommand<string>;  
}
