using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Services
{
    /// <summary>
    /// Implementación del motor de cálculo de tiempos de preparación.
    /// Estrategia: Botleneck (Cuello de botella) -> El pedido está listo 
    /// cuando el ítem más lento termina de prepararse.
    /// </summary>
    public class PrepTimeEngine : IPrepTimeEngine
    {
        public DateTime CalculateProjectedReadyAt(OrderGroup orderGroup)
        {
            if (orderGroup == null) throw new ArgumentNullException(nameof(orderGroup));

            // Si el cliente programó el pedido para el futuro, esa es nuestra base (no implementado aún, pero preparado)
            var baseTime = DateTime.UtcNow;

            int maxPrepMinutes = 0;

            // Recorremos todos los comercios del grupo (OrderGroup -> SubOrder -> Detail -> MerchantProduct)
            foreach (var subOrder in orderGroup.SubOrders)
            {
                var merchantDefault = subOrder.Merchant?.DefaultPreparationMinutes ?? 15;

                foreach (var detail in subOrder.Details)
                {
                    if (detail.MerchantProduct != null)
                    {
                        int productTime = GetProductPrepTime(detail.MerchantProduct, merchantDefault);
                        if (productTime > maxPrepMinutes)
                        {
                            maxPrepMinutes = productTime;
                        }
                    }
                }
            }

            // Aplicamos el tiempo de preparación al tiempo base
            return baseTime.AddMinutes(maxPrepMinutes);
        }

        public int GetProductPrepTime(MerchantProduct product, int merchantDefaultMinutes)
        {
            // Regla: Si el producto tiene tiempo específico lo usa, sino usa el default del comercio.
            // Si ambos son null (teóricamente imposible por el seed), fallback a 15 min.
            return product.PreparationMinutes ?? merchantDefaultMinutes;
        }
    }
}
