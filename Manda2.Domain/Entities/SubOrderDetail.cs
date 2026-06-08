using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class SubOrderDetail : BaseEntity
    {
        // ─── RELACIONES ───────────────────────────────────────────────────────
        public int SubOrderId { get; set; }
        public virtual SubOrder SubOrder { get; set; } = null!;

        // FK al catálogo vivo (para trazabilidad y reportes)
        public int MerchantProductId { get; set; }
        public virtual MerchantProduct MerchantProduct { get; set; } = null!;

        // ─── SNAPSHOT HISTÓRICO ───────────────────────────────────────────────
        // Estos campos guardan los valores AL MOMENTO de la compra.
        // Si el comercio cambia precio mañana, este pedido no se altera.
        public string ItemName { get; set; } = null!;
        public string? SnapshotImageUrl { get; set; }

        // ─── CANTIDADES ───────────────────────────────────────────────────────
        public int Quantity { get; set; }

        // ─── PRECIOS (FUENTE DE VERDAD HISTÓRICA) ────────────────────────────
        public decimal PurchasePrice { get; set; }      // Costo del comercio
        public decimal SalePrice { get; set; }          // Precio al cliente
        public decimal UnitDiscount { get; set; }       // Descuento por unidad (promo)
        public decimal NetSalePrice { get; set; }       // SalePrice - UnitDiscount

        // ─── COMISIÓN ─────────────────────────────────────────────────────────
        public decimal CommissionPct { get; set; }      // Snapshot de la comisión

        // ─── IMPUESTOS (INCLUIDOS EN SALE PRICE) ─────────────────────────────
        public decimal TaxPct { get; set; }
        public decimal TaxAmount { get; set; }          // Calculado: LineTotal × TaxPct

        // ─── TOTALES DE LÍNEA ─────────────────────────────────────────────────
        public decimal LineTotal { get; set; }          // NetSalePrice × Quantity
        public decimal LineTotalWithTax { get; set; }   // LineTotal + TaxAmount

        // ─── INSTRUCCIONES ESPECIALES ─────────────────────────────────────────
        // Ejemplo: "Sin cebolla", "Extra salsa", "Alérgico a maní"
        public string? Notes { get; set; }

        // ─── INTEGRACIÓN SAP B1 ───────────────────────────────────────────────
        public string? SAP_ItemCode { get; set; }

        //Documentación:
        // NetSalePrice    = SalePrice - UnitDiscount
        // LineTotal       = NetSalePrice × Quantity
        // TaxAmount       = LineTotal × (TaxPct / 100)
        // LineTotalWithTax = LineTotal + TaxAmount

        // SubOrder.SubTotal = SUM(SubOrderDetail.LineTotal)
        // SubOrder.TotalCommissionAmount = SubTotal × ResolvedCommissionPct
        // SubOrder.NetPayable = SubTotal - TotalCommissionAmount - DeliveryFeeProrrated
    }
}
