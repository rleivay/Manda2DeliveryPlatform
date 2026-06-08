// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetRejectionReasons/
//          RejectionReasonDto.cs
//
// PROPÓSITO: DTO del catálogo de motivos de rechazo para la app MAUI.
//            RequiresNote es el campo clave: cuando es true, la app activa
//            el campo de texto libre ReasonNotes en la pantalla.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Dtos
{
    /// <summary>
    /// Motivo de rechazo para poblar el Picker en la app del repartidor.
    /// </summary>
    public class RejectionReasonDto
    {
        /// <summary>ID del motivo. Se envía como ReasonId en RejectGroupCommand.</summary>
        public int Id { get; set; }

        /// <summary>Texto visible en el Picker de la app.</summary>
        public string DisplayName { get; set; } = null!;

        /// <summary>
        /// Si true, la app MAUI debe mostrar el campo ReasonNotes (texto libre).
        /// Si false, el campo ReasonNotes permanece oculto y no se envía.
        /// </summary>
        public bool RequiresNote { get; set; }
    }
}
