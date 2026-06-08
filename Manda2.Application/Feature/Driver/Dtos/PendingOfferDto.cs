using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    public class PendingOfferDto
    {
        public int OrderGroupId { get; set; }

        // Métrica económica
        public decimal DriverEarnings { get; set; } // Representa el DeliveryFee
        public string PaymentMethod { get; set; } = null!;
        public decimal AmountToCollect { get; set; } // Solo si es Efectivo

        // Métrica logística
        public int SubOrderCount { get; set; }
        public int TotalStops { get; set; }

        // Métrica de proximidad
        public double DistanceToFirstStopKm { get; set; }

        // Tiempos
        public DateTime OfferedAtUtc { get; set; }
        public int SecondsRemaining { get; set; } // Para el contador regresivo en la App

        // Información de mercaditos (com comercios nombre y tipo)
        public List<string> MerchantNames { get; set; } = new();
    }
}
