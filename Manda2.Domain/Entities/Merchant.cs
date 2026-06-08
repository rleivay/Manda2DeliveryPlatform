using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class Merchant : BaseEntity
    {
        // Datos del comercio
        public string Name { get; set; } = null!;
        public string CommercialPhone { get; set; } = null!;
        public string CommercialEmail { get; set; } = null!;

        public string AddressText { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // Adjuntos / Identidad del comercio (documento legal, contrato, etc.)
        public string? IdentityDocumentUrl { get; set; }
        public string? LogoUrl { get; set; }

        // Bancario (opcional)
        public string? BankAccountNumber { get; set; }
        public string? BankAccountType { get; set; }
        public string? BankName { get; set; }

        // Tipo de comercio para reglas de comisión
        public MerchantType Type { get; set; } = MerchantType.Marketplace;

        // Estado (BackOffice valida documentos)
        public bool IsActive { get; set; } = false;

        public decimal? CommissionPct { get; set; }

        
        // Aprobación BackOffice
        public bool IsApproved { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }

        // Estado operativo (abierto/cerrado)
        public bool IsOnline { get; set; } = false;

        // Categoría de comercio (para filtros en Client App)
        public int? MerchantCategoryId { get; set; }

        /// Tiempo de preparación por defecto para todos los productos de este comercio.
        /// Se usa como fallback cuando MerchantProduct.PreparationMinutes es null.
        /// Valor mínimo recomendado: 5 minutos.
        public int DefaultPreparationMinutes { get; set; } = 15;

        public virtual MerchantCategory? Category { get; set; }// Navigation property

        // Horarios
        public virtual ICollection<OperatingSchedule> OperatingSchedules { get; set; } = new List<OperatingSchedule>();
        public virtual ICollection<OperatingScheduleException> OperatingScheduleExceptions { get; set; } = new List<OperatingScheduleException>();
    }
}
