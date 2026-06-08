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

namespace Manda2.Application.Feature.Catalog.Commands.CreateMerchantCategory
{
    public class CreateMerchantCategoryHandler : ICommandHandler<CreateMerchantCategoryCommand, int>
    {
        private readonly IApplicationDbContext _db;

        public CreateMerchantCategoryHandler(IApplicationDbContext db) => _db = db;

        public async Task<int> HandleAsync(CreateMerchantCategoryCommand command, CancellationToken ct)
        {
            if (await _db.MerchantCategories.AnyAsync(x => x.Name == command.Name, ct))
                throw new BusinessRuleException($"La categoría de comercio '{command.Name}' ya existe.");

            var category = new MerchantCategory
            {
                Name = command.Name,
                IconUrl = command.IconUrl,
                IsActive = true
            };

            await _db.MerchantCategories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            return category.Id;
        }
    }
}
