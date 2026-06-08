using Manda2.Application.Mediator;
using Manda2.Contracts.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Queries.GetMerchantCategories
{
    /// <summary>
    /// Retorna todas las categorías de comercio activas (IsActive = true).
    /// Sin parámetros — catálogo completo para la app cliente.
    /// </summary>
    public record GetMerchantCategoriesQuery() : IQuery<List<MerchantCategoryDto>>;
}
