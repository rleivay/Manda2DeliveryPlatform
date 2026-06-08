using Manda2.Contracts.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Catalog
{
    /// <summary>
    /// Define las operaciones de consulta de catálogo para el cliente.
    /// </summary>
    public interface ICatalogService
    {
        /// <summary>
        /// Obtiene la lista de comercios abiertos que pueden recibir pedidos.
        /// Corresponde a: GET api/v1/catalog/merchants/open
        /// </summary>
        Task<List<MerchantSummaryDto>> GetOpenMerchantsAsync();

        /// <summary>
        /// Obtiene comercios filtrados por categoría.
        /// Corresponde a: GET api/v1/catalog/merchants?categoryId={id}
        /// </summary>
        Task<List<MerchantSummaryDto>> GetMerchantsByCategoryAsync(int? categoryId = null);

        /// <summary>
        /// Obtiene el menú de productos disponibles de un comercio específico.
        /// Corresponde a: GET api/v1/catalog/products/{merchantId}
        /// </summary>
        Task<List<MerchantProductDto>> GetMerchantProductsAsync(int merchantId);

        /// <summary>
        /// Obtiene el catálogo de categorías de comercio activas.
        /// Corresponde a: GET api/v1/catalog/merchant-categories
        /// </summary>
        Task<List<MerchantCategoryDto>> GetMerchantCategoriesAsync();

        Task<int> GetSystemConfigIntAsync(string key, int defaultValue);
    }
}
