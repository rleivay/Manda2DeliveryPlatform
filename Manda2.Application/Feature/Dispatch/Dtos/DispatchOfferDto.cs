using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Dtos
{
    /// <summary>
    /// DTO de oferta de dispatch enviada al driver por SignalR.
    /// </summary>
    public record DispatchOfferDto
    {
        public int OrderGroupId { get; init; }
        public int AttemptId { get; init; }
        public decimal PickupLatitude { get; init; }
        public decimal PickupLongitude { get; init; }
        public string MerchantName { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public int ExpiresInSeconds { get; init; }
    }
}
