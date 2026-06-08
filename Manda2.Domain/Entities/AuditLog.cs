using Manda2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class AuditLog:BaseEntity
    {
        // Nombre de la entidad afectada (e.g., "Merchant", "Customer", "Order")
        public string EntityName { get; set; } = null!;

        // Id de la entidad afectada (opcional si no aplica)
        public int? EntityId { get; set; }

        // Acción: Created, Updated, Deleted, DocumentUploaded, PaymentPosted, etc.
        public string Action { get; set; } = null!;

        // Información adicional (puede ser JSON con snapshot)
        public string? Data { get; set; }

        // Usuario que realizó la acción (opcional)
        public int? PerformedByUserId { get; set; }
        public string? PerformedBy { get; set; } // Texto descriptivo
        public string? Details { get; set; }     // Detalles adicionales

        //// Fecha y hora UTC
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// Indica que este log requiere atención inmediata del equipo operativo.
        /// Se activa cuando una comisión no puede resolverse en ningún nivel.
        /// </summary>
        public bool RequiresImmediateAttention { get; set; } = false;

        /// Categoría del log para filtrado en Backoffice.
        /// Ej: "COMMISSION_FALLBACK", "PRICE_CALC_ERROR", etc.
        /// </summary>
        public string? Category { get; set; }

        
    }
}
