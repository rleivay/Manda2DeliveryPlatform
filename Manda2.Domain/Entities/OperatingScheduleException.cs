using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class OperatingScheduleException : BaseEntity
    {
        public ActorType ActorType { get; set; }

        public int? MerchantId { get; set; }
        public virtual Merchant? Merchant { get; set; }

        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }

        public DateOnly Date { get; set; }
        public bool IsClosed { get; set; } = true;

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public int? ClosureReasonId { get; set; }
        public virtual ClosureReason? ClosureReason { get; set; }

        public string? Notes { get; set; }
    }
}
