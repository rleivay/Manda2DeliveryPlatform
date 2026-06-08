using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.UpdateMerchantSchedule
{
    public record UpdateMerchantScheduleCommand(
        int MerchantId,
        List<ScheduleDayDto> Days) : ICommand<string>;
}
