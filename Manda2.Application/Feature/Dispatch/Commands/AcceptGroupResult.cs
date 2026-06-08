using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    public record AcceptGroupResult(
         bool Success,
         string Message,
         int? OrderGroupId
     );
}
