using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Catalog
{
    /// <summary>
    /// Resumen de un comercio para listados y carruseles en la app cliente.
    /// </summary>
    public class MerchantSummaryDto
    {
        public int MerchantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int? CategoryId { get; set; }
        public decimal? CommissionPct { get; set; }
        public bool IsOpen { get; set; }
        public string? LogoUrl { get; set; }

        // ── Campos de ubicación (desde migración AddOrderGroupPayment) ──────────
        /// <summary>Dirección textual del comercio. Puede ser null si no fue cargada.</summary>
        public string? AddressText { get; set; }

        /// <summary>Latitud del comercio. 0 si no fue configurada.</summary>
        public decimal Latitude { get; set; }

        /// <summary>Longitud del comercio. 0 si no fue configurada.</summary>
        public decimal Longitude { get; set; }
    }
}
