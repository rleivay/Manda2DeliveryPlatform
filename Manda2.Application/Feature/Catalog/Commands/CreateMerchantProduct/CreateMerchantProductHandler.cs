using Manda2.Application.Common;
using Manda2.Application.Common.Exceptions;
using Manda2.Application.Feature.Catalog.Commands.CreateMerchantProduct;
using Manda2.Application.Mediator;
using Manda2.Application.Services;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;

public class CreateMerchantProductHandler
    : ICommandHandler<CreateMerchantProductCommand, CreateMerchantProductResult>
{
    private readonly IApplicationDbContext _db;

    public CreateMerchantProductHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CreateMerchantProductResult> HandleAsync(
        CreateMerchantProductCommand command,
        CancellationToken ct = default)
    {
        // ─── 1. CARGAR ENTIDADES ─────────────────────────────────────────
        var merchant = await _db.Merchants
            .FirstOrDefaultAsync(m => m.Id == command.MerchantId, ct)
            ?? throw new BusinessRuleException($"El comercio con Id {command.MerchantId} no existe.");

        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, ct)
            ?? throw new BusinessRuleException($"El producto con Id {command.ProductId} no existe.");

        // ─── 2. VALIDAR DUPLICADO ────────────────────────────────────────
        var exists = await _db.MerchantProducts
            .AnyAsync(mp => mp.MerchantId == command.MerchantId
                         && mp.ProductId == command.ProductId, ct);

        if (exists)
            throw new BusinessRuleException(
                $"El producto '{product.Name}' ya está registrado en el comercio '{merchant.Name}'.");

        // ─── 3. LEER COMISIÓN DEFAULT SEGÚN TIPO DE MERCHANT ────────────
        var configKey = merchant.Type switch
        {
            MerchantType.Marketplace => "COMMISSION_MARKETPLACE",
            MerchantType.DarkStore => "COMMISSION_DARKSTORE",
            MerchantType.DarkKitchen => "COMMISSION_DARKKITCHEN",
            _ => "COMMISSION_MARKETPLACE"
        };

        var defaultCommissionStr = await _db.AppConfigs
            .Where(c => c.Key == configKey)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(ct) ?? "0";

        var defaultCommission = decimal.Parse(defaultCommissionStr);

        // ─── 4. CONSTRUIR ENTITY TEMPORAL ───────────────────────────────
        var merchantProduct = new MerchantProduct
        {
            MerchantId = command.MerchantId,
            ProductId = command.ProductId,
            BasePrice = command.BasePrice,
            CommissionSource = command.CommissionSource,
            CommissionPctOverride = command.CommissionPctOverride,
            IsAvailable = false
        };

        // ─── 5. RESOLVER COMISIÓN ────────────────────────────────────────
        var resolver = new CommissionResolverService(defaultCommission);
        var resolution = resolver.Resolve(merchantProduct, product, merchant);

        // ─── 6. GUARDAR LOGS DE ALERTA ───────────────────────────────────
        if (resolution.Logs.Any())
            await _db.AuditLogs.AddRangeAsync(resolution.Logs, ct);

        // ─── 7. APLICAR RESULTADO ────────────────────────────────────────
        if (resolution.Resolved)
        {
            merchantProduct.ResolvedCommissionPct = resolution.CommissionPct;
            merchantProduct.ResolvedCommissionSource = resolution.Source;
            merchantProduct.SalePrice = command.BasePrice +
                (command.BasePrice * resolution.CommissionPct!.Value / 100);
            merchantProduct.IsAvailable = true;
            merchantProduct.ShowSamePriceLabel =
                merchantProduct.SalePrice == command.BasePrice;
        }
        else
        {
            merchantProduct.IsAvailable = false;
            merchantProduct.SalePrice = 0;
        }

        // ─── 8. PERSISTIR ────────────────────────────────────────────────
        await _db.MerchantProducts.AddAsync(merchantProduct, ct);
        await _db.SaveChangesAsync(ct);

        // ─── 9. RETORNAR CON TRAZABILIDAD ────────────────────────────────
        return new CreateMerchantProductResult(
            MerchantProductId: merchantProduct.Id,
            BasePrice: merchantProduct.BasePrice,
            SalePrice: merchantProduct.SalePrice,
            ResolvedCommissionPct: resolution.CommissionPct ?? 0,
            ResolvedCommissionSource: resolution.Source ?? command.CommissionSource,
            UsedFallback: resolution.UsedFallback,
            IsAvailable: merchantProduct.IsAvailable,
            Message: resolution.Resolved
                ? resolution.UsedFallback
                    ? "⚠️ Producto creado con comisión de fallback. Revisar AuditLog."
                    : "✅ Producto creado correctamente."
                : "⛔ Producto creado pero bloqueado. Comisión no resuelta en ningún nivel."
        );
    }
}