// PROPÓSITO:
//   Validaciones de entrada para StartDispatchCommand usando FluentValidation.
//   Se ejecuta antes de llegar al handler (pipeline behavior o llamada explícita).
// ═══════════════════════════════════════════════════════════════════════════
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Validador para StartDispatchCommand.
    /// Garantiza que los datos mínimos requeridos sean correctos
    /// antes de ejecutar la lógica de dispatch.
    /// </summary>
    public class StartDispatchCommandValidator : AbstractValidator<StartDispatchCommand>
    {
        public StartDispatchCommandValidator()
        {
            // OrderGroupId debe ser un entero positivo válido
            RuleFor(x => x.OrderGroupId)
                .GreaterThan(0)
                .WithMessage("OrderGroupId debe ser un identificador válido (> 0).");

            // ZoneName es opcional, pero si se envía no debe exceder 100 caracteres
            RuleFor(x => x.ZoneName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ZoneName))
                .WithMessage("ZoneName no debe superar 100 caracteres.");

            // InitiatedByUserId si se envía debe ser positivo
            RuleFor(x => x.InitiatedByUserId)
                .GreaterThan(0)
                .When(x => x.InitiatedByUserId.HasValue)
                .WithMessage("InitiatedByUserId debe ser un identificador válido (> 0) si se especifica.");
        }
    }
}
