// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/AcceptGroup/
//          AcceptGroupResult.cs
//
// PROPÓSITO: DTO de respuesta al driver tras aceptar el grupo.
//            Contiene la ruta completa ordenada para iniciar navegación.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Driver.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Resultado devuelto al driver tras aceptar exitosamente un grupo.
    /// La app del driver usa esto para iniciar la vista de navegación.
    /// </summary>
    public class AcceptGroupDriverResult
    {
        /// <summary>Confirmación de éxito.</summary>
        public bool Success { get; set; }

        /// <summary>Mensaje legible para mostrar en la app.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Lista de paradas en orden de visita.
        /// La app del driver abre Google Maps con estas coordenadas en secuencia.
        /// </summary>
        public List<StopSummaryDto> Stops { get; set; } = new();

        /// <summary>
        /// Monto total del pedido.
        /// Crítico si el pago es en efectivo: el driver sabe cuánto cobrar.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Método de pago del cliente.</summary>
        public string PaymentMethodName { get; set; } = string.Empty;
    }
}
