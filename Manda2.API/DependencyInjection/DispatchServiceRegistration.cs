// PROPÓSITO:
//   Registro de dependencias del módulo Dispatch que NO son handlers.
//   Los handlers (StartDispatchCommandHandler, CompleteStopCommandHandler)
//   son registrados automáticamente por AddManda2Mediator() — no duplicar aquí.
//
// CONTIENE:
//   - INotificationService → NotificationServiceStub (reemplazar en D3-C)
//   - IValidator<StartDispatchCommand> → StartDispatchCommandValidator
//
// LLAMAR DESDE Program.cs:
//   builder.Services.AddDispatchModule();
// ═══════════════════════════════════════════════════════════════════════════

using FluentValidation;
using Manda2.Application.Contracts;
using Manda2.Application.Feature.Dispatch.Commands;
using Manda2.Application.Feature.Finance.Commands;
using Manda2.Application.Feature.Payments.Commands;
using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;
using Manda2.Infrastructure.Notifications;

namespace Manda2.API.DependencyInjection
{
    public static class DispatchServiceRegistration
    {
        public static IServiceCollection AddDispatchModule(this IServiceCollection services)
        {
            // ── Handlers ──────────────────────────────────────────────────
            // ✅ NO registrar aquí — AddManda2Mediator() los registra automáticamente
            // por convención de interfaz ICommandHandler<TCommand, TResult>.

            // ── Validators (FluentValidation) ─────────────────────────────
            // FluentValidation NO es un handler del mediador → registro manual correcto.
            services.AddScoped<
                IValidator<StartDispatchCommand>,
                StartDispatchCommandValidator>();

            // ── Notification Service (Stub → reemplazar en D3-C) ──────────
            // INotificationService tampoco es handler del mediador → registro manual correcto.
            services.AddScoped<INotificationService, NotificationServiceStub>();

            services.AddScoped<
    ICommandHandler<ConfirmCheckoutCommand, ConfirmCheckoutResult>,
    ConfirmCheckoutCommandHandler>();

            services.AddScoped<
                ICommandHandler<ConfirmPaymentCommand, ConfirmPaymentResult>,
                ConfirmPaymentCommandHandler>();

            services.AddScoped<
    ICommandHandler<CustomerCreditNoteCommand, CustomerCreditNoteResult>,
    CustomerCreditNoteCommandHandler>();

            return services;
        }
    }
}
