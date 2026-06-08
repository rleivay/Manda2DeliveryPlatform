using Manda2.Domain.Common;
using Manda2.Contracts.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Domain.Entities
{
    public class SubOrder : BaseEntity
    {
        // ─── RELACIONES PRINCIPALES ──────────────────────────────────────────
        public int OrderGroupId { get; set; }
        
        public int MerchantId { get; set; }
        

        // ─── ESTADO OPERATIVO ────────────────────────────────────────────────
        public SubOrderStatus Status { get; set; }
            = SubOrderStatus.PendingMerchantAcceptance;

        // ─── FINANCIERO POR COMERCIO ─────────────────────────────────────────
        public decimal SubTotal { get; set; }

        // Comisión resuelta en cascada (MerchantProduct → Merchant → Category → AppConfig)
        public decimal ResolvedCommissionPct { get; set; }
        public decimal TotalCommissionAmount { get; set; }

        // Fee de delivery prorrateado entre SubOrders del mismo OrderGroup
        public decimal DeliveryFeeProrrated { get; set; }

        // Lo que realmente se le paga al comercio (SAP Ready)
        // NetPayable = SubTotal - TotalCommissionAmount - DeliveryFeeProrrated
        // Esto es lo que se liquida al comercio y lo que SAP B1 registrará como AP Invoice
        public decimal NetPayable { get; set; }

        // ── Auditoría de cálculo de delivery fee ──────────────────────────────
        // Snapshot inmutable al momento del checkout. No se modifica después.

        /// <summary>Distancia geodésica (Haversine) en km entre este comercio y el punto de entrega.</summary>
        public decimal? DeliveryDistanceKm { get; set; }

        /// <summary>Aporte porcentual de esta suborden al total de distancias del grupo (0-100).</summary>
        public decimal? DeliveryDistancePct { get; set; }

        /// <summary>Snapshot de DELIVERY_FEE_BASE usado al calcular.</summary>
        public decimal? SnapshotFeeBase { get; set; }

        /// <summary>Snapshot de DELIVERY_FEE_FREE_KM usado al calcular.</summary>
        public decimal? SnapshotFeeKmIncluidos { get; set; }

        /// <summary>Snapshot de DELIVERY_FEE_PER_KM usado al calcular.</summary>
        public decimal? SnapshotFeePorKm { get; set; }

        /// <summary>Distancia total del grupo al momento del cálculo (suma de todas las subórdenes).</summary>
        public decimal? SnapshotTotalGroupDistanceKm { get; set; }

        /// <summary>Fee total del grupo antes del prorrateo.</summary>
        public decimal? SnapshotTotalGroupFee { get; set; }

        // ─── TRAZABILIDAD OPERATIVA ──────────────────────────────────────────
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // ─── INTEGRACIÓN SAP B1 ──────────────────────────────────────────────
        public int? SAP_DocEntry_APInvoice { get; set; }

        // ─── COLECCIONES ─────────────────────────────────────────────────────
        public virtual ICollection<SubOrderDetail> Details { get; set; }
            = new List<SubOrderDetail>();
        public virtual Merchant Merchant { get; set; } = null!;
        public virtual OrderGroup OrderGroup { get; set; } = null!;
    }
}
