using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Enum
{
    public enum CommissionSource
    {
        Merchant = 1,   // Leer comisión del Merchant
        Category = 2,   // Leer comisión de la Categoría del Producto
        Product = 3    // Leer comisión directa del MerchantProduct (override)
    }
}
