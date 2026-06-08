using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Enum
{
    public enum SubOrderStatus
    {
        PendingMerchantAcceptance = 1,
        AcceptedByMerchant = 2,
        RejectedByMerchant = 3,
        Preparing = 4,
        ReadyForPickup = 5,
        PickedUp = 6,
        Delivered = 7,
        Cancelled = 8
    }
}
