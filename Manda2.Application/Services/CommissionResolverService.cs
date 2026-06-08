using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Services
{
    /// <summary>
    /// Resuelve la comisión aplicable a un MerchantProduct evaluando en cascada:
    /// Product → Category → Merchant → AppConfig (Default).
    /// Registra en AuditLog cada nivel vacío encontrado con flag de atención inmediata.
    /// Si ningún nivel tiene comisión, el producto NO puede ponerse a la venta.
    /// </summary>
    public class CommissionResolverService
    {
        private readonly decimal _defaultCommission;
        private readonly List<AuditLog> _pendingLogs = new();

        public CommissionResolverService(decimal defaultCommission)
        {
            _defaultCommission = defaultCommission;
        }

        /// <summary>
        /// Resultado de la resolución de comisión.
        /// </summary>
        public record CommissionResult(
            bool Resolved,
            decimal? CommissionPct,
            CommissionSource? Source,
            bool UsedFallback,
            bool BlockAvailability,
            IReadOnlyList<AuditLog> Logs
        );

        public CommissionResult Resolve(
            MerchantProduct merchantProduct,
            Product product,
            Merchant merchant)
        {
            _pendingLogs.Clear();

            var preferred = merchantProduct.CommissionSource;
            decimal? resolved = null;
            CommissionSource? resolvedSource = null;

            // ─── NIVEL 1: PRODUCT ───────────────────────────────────────────
            if (preferred == CommissionSource.Product)
            {
                if (merchantProduct.CommissionPctOverride.HasValue && merchantProduct.CommissionPctOverride > 0)
                {
                    resolved = merchantProduct.CommissionPctOverride;
                    resolvedSource = CommissionSource.Product;
                }
                else
                {
                    // ⚠️ Fallback: Product vacío → log + atención inmediata
                    AddAlert(
                        entityName: "MerchantProduct",
                        entityId: merchantProduct.Id,
                        message: $"CommissionSource=Product pero CommissionPctOverride está vacío. " +
                                 $"MerchantId={merchantProduct.MerchantId}, ProductId={merchantProduct.ProductId}",
                        category: "COMMISSION_FALLBACK"
                    );

                    // Intenta Category
                    if (product.Category?.CommissionPct.HasValue == true && product.Category.CommissionPct > 0)
                    {
                        resolved = product.Category.CommissionPct;
                        resolvedSource = CommissionSource.Category;
                    }
                    else
                    {
                        // ⚠️ Category también vacía
                        AddAlert(
                            entityName: "ProductCategory",
                            entityId: product.ProductCategoryId,
                            message: $"Fallback a Category pero CommissionPct está vacío. " +
                                     $"CategoryId={product.ProductCategoryId}",
                            category: "COMMISSION_FALLBACK"
                        );

                        // Intenta Merchant
                        resolved = ResolveFromMerchant(merchant, merchantProduct);
                        resolvedSource = resolved.HasValue ? CommissionSource.Merchant : null;
                    }
                }
            }

            // ─── NIVEL 2: CATEGORY ──────────────────────────────────────────
            else if (preferred == CommissionSource.Category)
            {
                if (product.Category?.CommissionPct.HasValue == true && product.Category.CommissionPct > 0)
                {
                    resolved = product.Category.CommissionPct;
                    resolvedSource = CommissionSource.Category;
                }
                else
                {
                    // ⚠️ Category vacía → log + atención inmediata
                    AddAlert(
                        entityName: "ProductCategory",
                        entityId: product.ProductCategoryId,
                        message: $"CommissionSource=Category pero CommissionPct está vacío. " +
                                 $"CategoryId={product.ProductCategoryId}",
                        category: "COMMISSION_FALLBACK"
                    );

                    // Intenta Merchant
                    resolved = ResolveFromMerchant(merchant, merchantProduct);
                    resolvedSource = resolved.HasValue ? CommissionSource.Merchant : null;
                }
            }

            // ─── NIVEL 3: MERCHANT ──────────────────────────────────────────
            else if (preferred == CommissionSource.Merchant)
            {
                resolved = ResolveFromMerchant(merchant, merchantProduct);
                resolvedSource = resolved.HasValue ? CommissionSource.Merchant : null;
            }

            // ─── TODOS VACÍOS: BLOQUEAR ─────────────────────────────────────
            if (!resolved.HasValue || resolved == 0)
            {
                AddAlert(
                    entityName: "MerchantProduct",
                    entityId: merchantProduct.Id,
                    message: $"⛔ COMISIÓN NO RESUELTA EN NINGÚN NIVEL. " +
                             $"Producto bloqueado para venta. " +
                             $"MerchantId={merchantProduct.MerchantId}, ProductId={merchantProduct.ProductId}",
                    category: "COMMISSION_UNRESOLVED",
                    isCritical: true
                );

                return new CommissionResult(
                    Resolved: false,
                    CommissionPct: null,
                    Source: null,
                    UsedFallback: true,
                    BlockAvailability: true,
                    Logs: _pendingLogs.AsReadOnly()
                );
            }

            return new CommissionResult(
                Resolved: true,
                CommissionPct: resolved,
                Source: resolvedSource,
                UsedFallback: resolvedSource != preferred,
                BlockAvailability: false,
                Logs: _pendingLogs.AsReadOnly()
            );
        }

        // ─── HELPERS ────────────────────────────────────────────────────────

        private decimal? ResolveFromMerchant(Merchant merchant, MerchantProduct merchantProduct)
        {
            if (merchant.CommissionPct.HasValue && merchant.CommissionPct > 0)
                return merchant.CommissionPct;

            // ⚠️ Merchant también vacío → log + atención inmediata
            AddAlert(
                entityName: "Merchant",
                entityId: merchant.Id,
                message: $"Fallback a Merchant pero CommissionPct está vacío. " +
                         $"MerchantId={merchant.Id}",
                category: "COMMISSION_FALLBACK"
            );

            // Último recurso: AppConfig default
            if (_defaultCommission > 0)
                return _defaultCommission;

            return null;
        }

        private void AddAlert(
            string entityName,
            int? entityId,
            string message,
            string category,
            bool isCritical = false)
        {
            _pendingLogs.Add(new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = "COMMISSION_RESOLUTION",
                Data = message,
                Category = category,
                RequiresImmediateAttention = true, // 🔥 Siempre alerta en fallback
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
