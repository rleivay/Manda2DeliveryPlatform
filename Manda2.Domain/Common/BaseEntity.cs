using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        // Soft delete flag
        public bool IsDeleted { get; set; } = false;

        // Auditoría temporal
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Auditoría por usuario (opcional - handlers deben asignarlos)
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
    }
}
