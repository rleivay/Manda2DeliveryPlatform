using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateMerchantProduct
{
    /// <summary>
    /// Command para registrar un producto en un comercio.
    /// El sistema resuelve automáticamente la comisión y calcula el SalePrice.
    /// </summary>
    public record CreateMerchantProductCommand(
        int MerchantId,
        int ProductId,
        decimal BasePrice,
        CommissionSource CommissionSource,
        decimal? CommissionPctOverride
    ) : ICommand<CreateMerchantProductResult>;
}
