using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateMerchantCategory
{
    public record CreateMerchantCategoryCommand(
        string Name,
    string? IconUrl) : ICommand<int>;
}
