using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateProductCategory
{
    public record CreateProductCategoryCommand(
        string Name,
        string? IconUrl,
        decimal? CommissionPct) : ICommand<int>;
}
