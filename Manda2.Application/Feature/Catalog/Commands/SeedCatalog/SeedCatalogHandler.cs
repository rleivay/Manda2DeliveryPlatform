using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.SeedCatalog
{
    public class SeedCatalogHandler : ICommandHandler<SeedCatalogCommand, string>
    {
        private readonly IApplicationDbContext _db;

        public SeedCatalogHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> HandleAsync(SeedCatalogCommand command, CancellationToken ct)
        {
            // 1. Verificar si ya hay datos para no duplicar
            if (await _db.MerchantCategories.AnyAsync(ct)) return "El Seed ya fue ejecutado previamente.";

            // 2. Crear Categoría de Comercio (Comisión 10%)
            var mCat = new MerchantCategory { Name = "Restaurantes", IsActive = true };
            _db.MerchantCategories.Add(mCat);

            // 3. Crear Categoría de Producto (Comisión 5%)
            var pCat = new ProductCategory { Name = "Hamburguesas", CommissionPct = 5.0m, IsActive = true };
            _db.ProductCategories.Add(pCat);

            await _db.SaveChangesAsync(ct);

            // 4. Crear Comercio (Asociado a "Restaurantes")
            var merchant = new Manda2.Domain.Entities.Merchant
            {
                Name = "Burger Master Test",
                MerchantCategoryId = mCat.Id,
                IsActive = true,
                IsApproved = true,
                Type = MerchantType.Marketplace,
                CommercialPhone = "555-0101",
                CommercialEmail = "test@burger.com",
                CommissionPct = 12.0m // Comisión a nivel Merchant
            };
            _db.Merchants.Add(merchant);

            // 5. Crear Producto Base (Asociado a "Hamburguesas")
            var product = new Product
            {
                Name = "Cheeseburger Clásica",
                Description = "Carne, Queso y Pan",
                ProductCategoryId = pCat.Id,
                IsActive = true
            };
            _db.Products.Add(product);

            await _db.SaveChangesAsync(ct);

            return $"Seed Exitoso. MerchantId: {merchant.Id}, ProductId: {product.Id}, PCatId: {pCat.Id}";
        }
    }
}
