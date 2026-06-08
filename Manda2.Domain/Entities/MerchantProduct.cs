using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class MerchantProduct : BaseEntity
    {
        public int MerchantId { get; set; }
        public int ProductId { get; set; }

        // Configuración de Precio
        public decimal BasePrice { get; set; } // Lo que cobra el comercio
        public decimal SalePrice { get; set; } // Lo que ve el cliente (Base + Comisión)

        public CommissionSource CommissionSource { get; set; }// El sistema evalúa en cascada si el nivel preferido está vacío.

        /// Comisión directa del producto. Solo aplica si CommissionSource = Product.
        /// Si está vacío, el sistema hace fallback a Category → Merchant → AppConfig.
        public decimal? CommissionPctOverride { get; set; }/// Si está vacío, el sistema hace fallback a Category → Merchant → AppConfig.

        /// Comisión efectivamente resuelta y aplicada al calcular SalePrice.
        /// Se guarda para trazabilidad y auditoría.
        public decimal? ResolvedCommissionPct { get; set; }

        /// Origen real desde donde se resolvió la comisión.
        /// Puede diferir de CommissionSource si hubo fallback.
        /// </summary>
        public CommissionSource? ResolvedCommissionSource { get; set; }
        public bool ShowSamePriceLabel { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navegación
        public virtual Merchant Merchant { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;

        /// Tiempo de preparación específico de este producto en este comercio (en minutos).
        /// Si es null, el sistema usa Merchant.DefaultPreparationMinutes como fallback.
        public int? PreparationMinutes { get; set; }
    }
}
