using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Domain.Entities
{
    public class OrderGroupStop : BaseEntity
    {
        // ─── RELACIONES ───────────────────────────────────────────────────────
        public int OrderGroupId { get; set; }
        public virtual OrderGroup OrderGroup { get; set; } = null!;

        // Null si es Dropoff (entrega al cliente)
        public int? MerchantId { get; set; }
        public virtual Merchant? Merchant { get; set; }

        // ─── TIPO Y SECUENCIA ─────────────────────────────────────────────────
        public StopType StopType { get; set; }

        // Orden de visita en la ruta: 1 = primer pickup, N = dropoff final
        public int Sequence { get; set; }

        // ─── UBICACIÓN ────────────────────────────────────────────────────────
        public string AddressText { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // ─── TIEMPOS ESTIMADOS vs REALES ─────────────────────────────────────
        public DateTime? EstimatedArrivalAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // ─── ESTADO ───────────────────────────────────────────────────────────
        public bool IsCompleted { get; set; } = false;

        // ─── NOTAS OPERATIVAS ─────────────────────────────────────────────────
        // Instrucciones especiales del cliente para esta parada
        public string? Notes { get; set; }
    }
}
