using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Catalog
{
    /// <summary>
    /// DTO de resumen de una categoría de comercio.
    /// Expone solo los campos necesarios para la app cliente.
    /// </summary>
    public class MerchantCategoryDto
    {
        /// <summary>PK de cfg.MerchantCategories.</summary>
        public int Id { get; set; }

        /// <summary>Nombre visible de la categoría. Ej: "Restaurantes", "Farmacias".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// URL del ícono de la categoría.
        /// Null si no tiene ícono asignado — la UI mostrará un ícono genérico.
        /// </summary>
        public string? IconUrl { get; set; }
    }
}
