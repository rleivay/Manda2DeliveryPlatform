using Manda2.Application.DTOs;
using Manda2.Application.Mediator;
using Manda2.Contracts.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Queries.GetMerchantsByCategory
{
    public record GetMerchantsByCategoryQuery(int? CategoryId)
        : IQuery<List<MerchantSummaryDto>>;
}
