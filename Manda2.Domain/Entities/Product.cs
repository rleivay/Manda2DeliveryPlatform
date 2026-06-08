using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int ProductCategoryId { get; set; }
        public bool IsActive { get; set; } = true;

        // Propiedades de Navegación
        public virtual ProductCategory Category { get; set; } = null!;
    }
}
