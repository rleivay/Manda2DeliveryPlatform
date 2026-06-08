using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal? CommissionPct { get; set; }
    }
}
