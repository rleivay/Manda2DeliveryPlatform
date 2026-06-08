using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Catalog
{
    public class MerchantProductDto
    {
        public int MerchantProductId { get; set; }
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public string? CategoryName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public bool ShowSamePriceLabel { get; set; }
        public bool IsAvailable { get; set; }
        public string? CommissionSource { get; set; }
        public decimal? ResolvedCommissionPct { get; set; }
    }
}
