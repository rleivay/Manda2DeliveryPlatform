using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Services
{
    /// <summary>
    /// Motor encargado de calcular los tiempos de preparación estimados (ETAs) 
    /// basados en el catálogo de productos y el histórico del comercio.
    /// </summary>
    public interface IPrepTimeEngine
    {
        /// <summary>
        /// Calcula la fecha y hora proyectada (UTC) en que un OrderGroup estará listo.
        /// Toma el producto con el mayor tiempo de preparación de todo el grupo.
        /// </summary>
        /// <param name="orderGroup">Grupo de pedidos a evaluar.</param>
        /// <returns>DateTime con la proyección ReadyAt UTC.</returns>
        DateTime CalculateProjectedReadyAt(OrderGroup orderGroup);

        /// <summary>
        /// Determina el tiempo de preparación de un producto específico, 
        /// resolviendo la jerarquía: Producto ? Merchant Default.
        /// </summary>
        int GetProductPrepTime(MerchantProduct product, int merchantDefaultMinutes);
    }
}
