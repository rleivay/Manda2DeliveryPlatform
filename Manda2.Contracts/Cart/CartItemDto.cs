using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Cart
{
    public class CartItemDto
    {
        /// <summary>PK de SubOrderDetail — usado para editar/eliminar el item.</summary>
        public int SubOrderDetailId { get; set; }

        /// <summary>Snapshot del nombre del producto al momento de agregar.</summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>Snapshot de imagen del producto.</summary>
        public string? SnapshotImageUrl { get; set; }

        public int Quantity { get; set; }

        /// <summary>Precio neto por unidad (SalePrice - UnitDiscount).</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>LineTotal = NetSalePrice × Quantity.</summary>
        public decimal LineTotal { get; set; }

        /// <summary>LineTotalWithTax = LineTotal + TaxAmount.</summary>
        public decimal LineTotalWithTax { get; set; }

        /// <summary>Instrucciones especiales del cliente. Ej: "Sin cebolla".</summary>
        public string? Notes { get; set; }
    }
}
