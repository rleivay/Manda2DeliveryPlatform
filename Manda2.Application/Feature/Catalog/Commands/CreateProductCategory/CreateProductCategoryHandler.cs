using Manda2.Application.Common;
using Manda2.Application.Common.Exceptions;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.CreateProductCategory
{
    public class CreateProductCategoryHandler : ICommandHandler<CreateProductCategoryCommand, int>
    {
        private readonly IApplicationDbContext _db;

        public CreateProductCategoryHandler(IApplicationDbContext db) => _db = db;

        public async Task<int> HandleAsync(CreateProductCategoryCommand command, CancellationToken ct)
        {
            if (await _db.ProductCategories.AnyAsync(x => x.Name == command.Name, ct))
                throw new BusinessRuleException($"La categoría de producto '{command.Name}' ya existe.");

            var category = new ProductCategory
            {
                Name = command.Name,
                IconUrl = command.IconUrl,
                CommissionPct = command.CommissionPct,
                IsActive = true
            };

            await _db.ProductCategories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            return category.Id;
        }
    }
}
