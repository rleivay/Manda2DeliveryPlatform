using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    public class DriverProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Estado operativo: Available, Busy, Offline, Suspended.</summary>
        public string Status { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public bool IsCashBlocked { get; set; }

        /// <summary>OrderGroup activo asignado. Null si está libre.</summary>
        public int? CurrentOrderGroupId { get; set; }

        /// <summary>Efectivo acumulado pendiente de liquidar.</summary>
        public decimal CurrentCashBalance { get; set; }

        /// <summary>Límite máximo de efectivo antes de bloqueo.</summary>
        public decimal MaxCashLimit { get; set; }

        /// <summary>Máximo de grupos activos simultáneos (Fase 2).</summary>
        public int MaxActiveGroups { get; set; }

        /// <summary>Máximo de SubOrders por grupo.</summary>
        public int MaxSubOrderLimit { get; set; }

        /// <summary>Código de empleado en SAP B1.</summary>
        public string? SapEmployeeCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
