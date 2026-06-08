using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Cart
{
    /// <summary>
    /// Carrito en memoria para la sesión del cliente.
    /// Thread-safe para operaciones de UI (Blazor es single-threaded en WebView).
    /// </summary>
    public class CartService : ICartService
    {
        // ── Estado interno ────────────────────────────────────────────────────
        private readonly List<CartStateMerchant> _merchants = new();
        private int _merchantLimit = 1; // default conservador hasta que se inicialice

        // ── ICartService: Estado ──────────────────────────────────────────────

        public IReadOnlyList<CartStateMerchant> Merchants => _merchants.AsReadOnly();

        public decimal GrandTotal => _merchants.Sum(m => m.SubTotal);

        public int TotalItemCount => _merchants.Sum(m => m.Items.Sum(i => i.Quantity));

        public int MerchantLimit => _merchantLimit;

        public bool IsAtMerchantLimit =>
            _merchants.Count >= _merchantLimit;

        // ── ICartService: Eventos ─────────────────────────────────────────────

        public event Action OnCartChanged = delegate { };

        // ── ICartService: Operaciones ─────────────────────────────────────────

        /// <inheritdoc/>
        public Task InitializeAsync(int merchantLimitFromConfig)
        {
            _merchantLimit = merchantLimitFromConfig > 0 ? merchantLimitFromConfig : 1;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public AddToCartResult AddItem(
            int merchantId,
            string merchantName,
            string? logoBase64,
            CartStateItem item)
        {
            // Buscar bloque del comercio
            var merchantCart = _merchants.FirstOrDefault(m => m.MerchantId == merchantId);

            if (merchantCart == null)
            {
                // El comercio no está en el carrito — verificar límite
                if (_merchants.Count >= _merchantLimit)
                    return AddToCartResult.MerchantLimitReached;

                // Crear bloque nuevo para este comercio
                merchantCart = new CartStateMerchant
                {
                    MerchantId = merchantId,
                    MerchantName = merchantName,
                    LogoBase64 = logoBase64
                };
                _merchants.Add(merchantCart);
            }

            // Buscar si el producto ya existe en el bloque
            var existing = merchantCart.Items
                .FirstOrDefault(i => i.ProductId == item.ProductId);

            if (existing != null)
            {
                // Incrementar cantidad
                existing.Quantity += item.Quantity;
            }
            else
            {
                // Agregar nueva línea
                merchantCart.Items.Add(item);
            }

            NotifyChanged();
            return AddToCartResult.Success;
        }

        /// <inheritdoc/>
        public void RemoveOneItem(int merchantId, int productId)
        {
            var merchantCart = _merchants.FirstOrDefault(m => m.MerchantId == merchantId);
            if (merchantCart == null) return;

            var item = merchantCart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;

            item.Quantity--;

            if (item.Quantity <= 0)
                merchantCart.Items.Remove(item);

            // Si el comercio quedó sin items, eliminarlo del carrito
            if (!merchantCart.Items.Any())
                _merchants.Remove(merchantCart);

            NotifyChanged();
        }

        /// <inheritdoc/>
        public void RemoveItem(int merchantId, int productId)
        {
            var merchantCart = _merchants.FirstOrDefault(m => m.MerchantId == merchantId);
            if (merchantCart == null) return;

            var item = merchantCart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
                merchantCart.Items.Remove(item);

            if (!merchantCart.Items.Any())
                _merchants.Remove(merchantCart);

            NotifyChanged();
        }

        /// <inheritdoc/>
        public void RemoveMerchant(int merchantId)
        {
            var merchantCart = _merchants.FirstOrDefault(m => m.MerchantId == merchantId);
            if (merchantCart != null)
            {
                _merchants.Remove(merchantCart);
                NotifyChanged();
            }
        }

        /// <inheritdoc/>
        public void UpdateItemNote(int merchantId, int productId, string? note)
        {
            var item = _merchants
                .FirstOrDefault(m => m.MerchantId == merchantId)
                ?.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.SpecialInstructions = note;
                NotifyChanged();
            }
        }

        /// <inheritdoc/>
        public int GetItemQuantity(int merchantId, int productId)
        {
            return _merchants
                .FirstOrDefault(m => m.MerchantId == merchantId)
                ?.Items.FirstOrDefault(i => i.ProductId == productId)
                ?.Quantity ?? 0;
        }

        /// <inheritdoc/>
        public CartStateMerchant? GetMerchantCart(int merchantId)
            => _merchants.FirstOrDefault(m => m.MerchantId == merchantId);

        /// <inheritdoc/>
        public bool HasMerchant(int merchantId)
            => _merchants.Any(m => m.MerchantId == merchantId);

        /// <inheritdoc/>
        public void Clear()
        {
            _merchants.Clear();
            NotifyChanged();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void NotifyChanged() => OnCartChanged.Invoke();
    }
}
