using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateMerchantProduct
{
    public record CreateMerchantProductResult(
    int MerchantProductId,
    decimal BasePrice,
    decimal SalePrice,
    decimal ResolvedCommissionPct,
    CommissionSource ResolvedCommissionSource,
    bool UsedFallback,
    bool IsAvailable,
    string Message
);
}
