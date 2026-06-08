using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class OperatingSchedule : BaseEntity
    {
        public ActorType ActorType { get; set; }

        // FKs opcionales según el tipo
        public int? MerchantId { get; set; }
        public virtual Merchant? Merchant { get; set; }

        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        public bool IsClosed { get; set; } = false;

        public int? ClosureReasonId { get; set; }
        public virtual ClosureReason? ClosureReason { get; set; }
    }
}
