using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manda2.Application.DTOs;
using Manda2.Contracts.Catalog;

namespace Manda2.Application.Feature.Catalog.Queries.GetMerchantProducts
{
    public record GetMerchantProductsQuery(int MerchantId)
        : IQuery<List<MerchantProductDto>>;
}
