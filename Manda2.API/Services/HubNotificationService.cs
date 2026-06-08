// PROPÓSITO: Servicio que los HANDLERS usan para enviar notificaciones
//            SignalR sin depender directamente del Hub.
//
// PATRÓN: IHubContext<Manda2Hub> inyectado en el servicio.
//         Los handlers inyectan IHubNotificationService.
//         Desacoplamiento total: Application no referencia SignalR.
//
// CONSUMIDORES (handlers que ya existen):
//   - StartDispatchCommandHandler  → NotifyDispatchOfferAsync
//   - AcceptGroupDriverCommandHandler → NotifyOrderGroupStatusAsync
//   - CompleteStopCommandHandler   → NotifyOrderGroupStatusAsync
//   - AcceptSubOrderCommandHandler → NotifySubOrderStatusAsync
//   - MarkSubOrderReadyCommandHandler → NotifySubOrderStatusAsync
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.API.Hubs;
using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace Manda2.API.Services
{
    /// <summary>
    /// Implementación de IHubNotificationService usando SignalR.
    /// Registrado como Scoped en DI.
    /// </summary>
    public class HubNotificationService : IHubNotificationService
    {
        private readonly IHubContext<Manda2Hub> _hub;

        public HubNotificationService(IHubContext<Manda2Hub> hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// Notifica a todos los conectados al grupo que el estado cambió.
        /// Evento: "OrderGroupStatusChanged"
        /// </summary>
        public async Task NotifyOrderGroupStatusAsync(
            int orderGroupId, string newStatus, CancellationToken ct = default)
        {
            await _hub.Clients
                .Group($"group_{orderGroupId}")
                .SendAsync("OrderGroupStatusChanged",
                    new { OrderGroupId = orderGroupId, Status = newStatus },
                    ct);

            // BackOffice siempre recibe todos los cambios de estado
            await _hub.Clients
                .Group("backoffice")
                .SendAsync("OrderGroupStatusChanged",
                    new { OrderGroupId = orderGroupId, Status = newStatus },
                    ct);
        }

        /// <summary>
        /// Notifica al comercio y al grupo que una SubOrder cambió de estado.
        /// Evento: "SubOrderStatusChanged"
        /// </summary>
        public async Task NotifySubOrderStatusAsync(
            int orderGroupId, int subOrderId, int merchantId,
            string newStatus, CancellationToken ct = default)
        {
            var payload = new
            {
                OrderGroupId = orderGroupId,
                SubOrderId = subOrderId,
                MerchantId = merchantId,
                Status = newStatus
            };

            await _hub.Clients
                .Group($"group_{orderGroupId}")
                .SendAsync("SubOrderStatusChanged", payload, ct);

            await _hub.Clients
                .Group($"merchant_{merchantId}")
                .SendAsync("SubOrderStatusChanged", payload, ct);

            await _hub.Clients
                .Group("backoffice")
                .SendAsync("SubOrderStatusChanged", payload, ct);
        }

        /// <summary>
        /// Envía una oferta de dispatch al driver específico.
        /// Evento: "DispatchOfferReceived"
        /// </summary>
        public async Task NotifyDispatchOfferAsync(
            int driverId, DispatchOfferDto offer, CancellationToken ct = default)
        {
            await _hub.Clients
                .Group($"driver_{driverId}")
                .SendAsync("DispatchOfferReceived", offer, ct);
        }

        /// <summary>
        /// Notifica al cliente que su driver está en camino.
        /// Evento: "DriverEnRoute"
        /// </summary>
        public async Task NotifyDriverEnRouteAsync(
            int orderGroupId, int driverId, CancellationToken ct = default)
        {
            await _hub.Clients
                .Group($"group_{orderGroupId}")
                .SendAsync("DriverEnRoute",
                    new { OrderGroupId = orderGroupId, DriverId = driverId },
                    ct);
        }
    }
}
