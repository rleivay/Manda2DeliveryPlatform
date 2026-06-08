using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Cart
{
    /// <summary>
    /// Representa una línea de producto dentro del carrito de un comercio.
    /// Mapea 1:1 con CartItemDto al momento del checkout.
    /// </summary>
    public class CartStateItem
    {
        /// <summary>ProductId del MerchantProduct (campo ProductId en CartItemDto).</summary>
        public int ProductId { get; set; }

        /// <summary>MerchantProductId — para validación visual y snapshot.</summary>
        public int MerchantProductId { get; set; }

        /// <summary>Nombre del producto — solo para display, no va al backend.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL de imagen — reservado para uso futuro cuando SSL esté configurado.
        /// En MVP usar ImageBase64.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Imagen del producto en Base64 (data URI completo).
        /// Mismo patrón que CartStateMerchant.LogoBase64.
        /// Usado para display en CartPage sin depender de SSL ni URLs relativas.
        /// Ejemplo: "data:image/jpeg;base64,/9j/4AAQ..."
        /// </summary>
        public string? ImageBase64 { get; set; }

        /// <summary>Precio de venta resuelto (SalePrice del MerchantProductDto).</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Cantidad seleccionada. Mínimo 1.</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Nota del cliente para esta línea.
        /// Mapea a CartItemDto.SpecialInstructions al hacer checkout.
        /// </summary>
        public string? SpecialInstructions { get; set; }

        /// <summary>Total calculado de la línea (UnitPrice * Quantity).</summary>
        public decimal LineTotal => UnitPrice * Quantity;
    }

    /// <summary>
    /// Agrupa las líneas de un comercio dentro del carrito.
    /// Mapea 1:1 con MerchantCartDto al momento del checkout.
    /// </summary>
    public class CartStateMerchant
    {
        /// <summary>Id del comercio.</summary>
        public int MerchantId { get; set; }

        /// <summary>Nombre del comercio — solo para display.</summary>
        public string MerchantName { get; set; } = string.Empty;

        /// <summary>Logo base64 — solo para display en CartBar y pantalla de carrito.</summary>
        public string? LogoBase64 { get; set; }

        /// <summary>Líneas de productos de este comercio.</summary>
        public List<CartStateItem> Items { get; set; } = new();

        /// <summary>Subtotal del comercio (suma de LineTotals).</summary>
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
    }
}
