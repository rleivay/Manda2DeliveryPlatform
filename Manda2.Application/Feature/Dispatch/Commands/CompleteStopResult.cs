using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Resultado del comando CompleteStop.
    /// Información mínima que necesita el caller para UX.
    /// </summary>
    public class CompleteStopResult
    {
        public int StopId { get; set; }
        public string StopType { get; set; } = default!;
        public int OrderGroupId { get; set; }
        public bool OrderGroupCompleted { get; set; }
        public string Message { get; set; } = default!;
    }
}
