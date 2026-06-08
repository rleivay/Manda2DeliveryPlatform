using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Cart
{
    /// <summary>
    /// Resultado de intentar agregar un item al carrito.
    /// </summary>
    public enum AddToCartResult
    {
        /// <summary>Item agregado o cantidad actualizada correctamente.</summary>
        Success,

        /// <summary>
        /// El comercio no está en el carrito y agregar uno nuevo excedería el límite.
        /// La UI debe mostrar notificación de límite alcanzado.
        /// </summary>
        MerchantLimitReached,

        /// <summary>El producto no está disponible.</summary>
        ProductUnavailable
    }

    /// <summary>
    /// Servicio de carrito en memoria para la sesión del cliente.
    /// Soporta múltiples comercios (OrderGroup multi-SubOrder).
    /// El límite de comercios se lee desde configuración (DEFAULT_DRIVER_MAX_SUBORDER_LIMIT).
    /// </summary>
    public interface ICartService
    {
        // ── Estado ────────────────────────────────────────────────────────────

        /// <summary>Lista de comercios actualmente en el carrito.</summary>
        IReadOnlyList<CartStateMerchant> Merchants { get; }

        /// <summary>Total global del carrito (suma de SubTotals de todos los comercios).</summary>
        decimal GrandTotal { get; }

        /// <summary>Cantidad total de items en el carrito (suma de Quantities).</summary>
        int TotalItemCount { get; }

        /// <summary>Límite máximo de comercios permitidos (leído desde config).</summary>
        int MerchantLimit { get; }

        /// <summary>True si el carrito ya alcanzó el límite de comercios.</summary>
        bool IsAtMerchantLimit { get; }

        // ── Eventos ───────────────────────────────────────────────────────────

        /// <summary>
        /// Se dispara cuando el estado del carrito cambia.
        /// Los componentes suscritos deben llamar StateHasChanged().
        /// </summary>
        event Action OnCartChanged;

        // ── Operaciones ───────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa el límite de comercios desde la API de configuración.
        /// Debe llamarse una vez al iniciar la sesión del cliente.
        /// </summary>
        Task InitializeAsync(int merchantLimitFromConfig);

        /// <summary>
        /// Intenta agregar o incrementar un producto en el carrito.
        /// Retorna AddToCartResult para que la UI decida cómo notificar.
        /// </summary>
        AddToCartResult AddItem(
            int merchantId,
            string merchantName,
            string? logoBase64,
            CartStateItem item);

        /// <summary>
        /// Reduce en 1 la cantidad de un producto. Si llega a 0, lo elimina.
        /// </summary>
        void RemoveOneItem(int merchantId, int productId);

        /// <summary>
        /// Elimina completamente un producto del carrito de un comercio.
        /// Si el comercio queda sin items, se elimina del carrito.
        /// </summary>
        void RemoveItem(int merchantId, int productId);

        /// <summary>
        /// Elimina todos los productos de un comercio del carrito.
        /// </summary>
        void RemoveMerchant(int merchantId);

        /// <summary>
        /// Actualiza la nota (SpecialInstructions) de un item.
        /// </summary>
        void UpdateItemNote(int merchantId, int productId, string? note);

        /// <summary>
        /// Retorna la cantidad actual de un producto en el carrito de un comercio.
        /// 0 si no está en el carrito.
        /// </summary>
        int GetItemQuantity(int merchantId, int productId);

        /// <summary>
        /// Retorna el bloque del comercio si existe en el carrito. Null si no.
        /// </summary>
        CartStateMerchant? GetMerchantCart(int merchantId);

        /// <summary>
        /// True si el comercio indicado ya está en el carrito.
        /// </summary>
        bool HasMerchant(int merchantId);

        /// <summary>
        /// Vacía completamente el carrito.
        /// </summary>
        void Clear();
    }
}
