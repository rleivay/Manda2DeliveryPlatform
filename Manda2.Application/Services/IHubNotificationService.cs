// PROPÓSITO: Contrato de notificaciones en tiempo real.
//            Vive en Application — sin referencia a SignalR.
//            La implementación concreta (HubNotificationService) vive en API.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Dispatch.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Services
{
    /// <summary>
    /// Contrato para enviar notificaciones en tiempo real a los actores.
    /// </summary>
    public interface IHubNotificationService
    {
        Task NotifyOrderGroupStatusAsync(
            int orderGroupId, string newStatus, CancellationToken ct = default);

        Task NotifySubOrderStatusAsync(
            int orderGroupId, int subOrderId, int merchantId,
            string newStatus, CancellationToken ct = default);

        Task NotifyDispatchOfferAsync(
            int driverId, DispatchOfferDto offer, CancellationToken ct = default);

        Task NotifyDriverEnRouteAsync(
            int orderGroupId, int driverId, CancellationToken ct = default);
    }
}
