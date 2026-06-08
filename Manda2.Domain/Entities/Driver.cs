using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class Driver : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string IdentityDocumentUrl { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public bool IsOnline { get; set; } = false;

        public DriverStatus Status { get; set; } // Enum (ver punto 3)

        // Finanzas
        public decimal MaxCashLimit { get; set; }
        public decimal CurrentCashBalance { get; set; }
        public bool IsCashBlocked { get; set; }

        // Bancario (opcional)
        public string? BankAccountNumber { get; set; }
        public string? BankAccountType { get; set; }
        public string? BankName { get; set; }

        // Logística
        public int MaxActiveGroups { get; set; } = 3;

        // SAP Ready
        public string? SAP_EmployeeCode { get; set; }

        
        // RELACIONES
        public ICollection<CashDeposit> CashDeposits { get; set; } = new List<CashDeposit>();

        //Horarios
        public virtual ICollection<OperatingSchedule> OperatingSchedules { get; set; } = new List<OperatingSchedule>();
        public virtual ICollection<OperatingScheduleException> OperatingScheduleExceptions { get; set; } = new List<OperatingScheduleException>();

        // CAPACIDAD (Punto clave de ayer)
        public int MaxSubOrderLimit { get; set; } = 1;

        // ESTADO ACTUAL
        public int? CurrentOrderGroupId { get; set; } = 0; // Si tiene valor, está ocupado (Un solo grupo a la vez)

        // POSICIÓN (Para el algoritmo de cercanía)
        public decimal? LastLatitude { get; set; }
        public decimal? LastLongitude { get; set; }
        public DateTime? LastLocationUpdateAt { get; set; }

    }
}
