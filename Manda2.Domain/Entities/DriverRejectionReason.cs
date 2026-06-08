// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Domain/Entities/DriverRejectionReason.cs
//
// PROPÓSITO: Catálogo administrable de motivos de rechazo de ofertas.
//            Permite al BackOffice gestionar los motivos sin tocar código.
//
// CAMPOS CLAVE:
//   - RequiresNote: si true, la app MAUI activa el campo ReasonNotes libre.
//   - IsActive: permite desactivar motivos sin borrarlos (auditoría).
//   - DisplayOrder: controla el orden visual en el Picker de MAUI.
//
// ADMINISTRACIÓN: BackOffice → Configuración → Motivos de Rechazo
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Domain.Common;

namespace Manda2.Domain.Entities
{
    /// <summary>
    /// Catálogo de motivos de rechazo de ofertas de pedido por parte del repartidor.
    /// Administrable desde BackOffice sin necesidad de cambios en código.
    /// </summary>
    public class DriverRejectionReason : BaseEntity
    {
        /// <summary>
        /// Código interno único del motivo. Útil para lógica condicional
        /// sin depender del Id numérico.
        /// Ejemplos: "TOO_FAR", "LOW_EARNINGS", "UNSAFE_ZONE", "OTHER"
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Texto visible en la app del repartidor (Picker/CollectionView).
        /// Ejemplos: "Muy lejos", "Ganancia baja", "Zona insegura", "Otro motivo"
        /// </summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>
        /// Si es true, la app MAUI activa el campo de texto libre (ReasonNotes).
        /// Típicamente true solo para el motivo "Otro motivo" (Code = "OTHER").
        /// </summary>
        public bool RequiresNote { get; set; } = false;

        /// <summary>
        /// Controla el orden de aparición en el Picker de la app.
        /// Menor número = aparece primero.
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Permite desactivar un motivo sin eliminarlo.
        /// Los motivos inactivos no aparecen en la app pero se conservan
        /// en registros históricos de rechazo.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // ─── NAVEGACIÓN INVERSA ───────────────────────────────────────────
        /// <summary>
        /// Historial de rechazos que usaron este motivo.
        /// Útil para reportes y analítica operativa.
        /// </summary>
        public virtual ICollection<DispatchAttemptDriver> RejectionHistory { get; set; } = new List<DispatchAttemptDriver>();
    }
}