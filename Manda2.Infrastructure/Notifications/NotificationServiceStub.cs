// PROPÓSITO:
//   Implementación Stub de INotificationService para desarrollo y tests.
//   No envía notificaciones reales — solo loguea en consola/ILogger.
//   Reemplazar por SignalRNotificationService en Sprint D3-C (producción).
//
// USO EN TESTS:
//   var stub = new NotificationServiceStub(db, NullLogger<NotificationServiceStub>.Instance);
//
// USO EN API (desarrollo):
//   services.AddScoped<INotificationService, NotificationServiceStub>();

using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Infrastructure.Notifications
{
    //// <summary>
    /// Stub de notificaciones para desarrollo y pruebas unitarias.
    /// Registra cada llamada en el logger sin efectos secundarios reales.
    /// </summary>
    public class NotificationServiceStub : INotificationService
    {
        private readonly ILogger<NotificationServiceStub> _logger;

        public NotificationServiceStub(ILogger<NotificationServiceStub> logger)
        {
            _logger = logger;
        }

        // ── Método 1 ──────────────────────────────────────────────────────
        public Task NotifyDriversAsync(
            IEnumerable<(int DriverId, double DistanceKm, int EtaMinutes)> drivers,
            int orderGroupId,
            int attemptId,
            CancellationToken cancellationToken = default)
        {
            foreach (var (driverId, distanceKm, etaMinutes) in drivers)
            {
                _logger.LogInformation(
                    "[STUB] NotifyDrivers → OrderGroup:{OrderGroupId} | Attempt:{AttemptId} | " +
                    "Driver:{DriverId} | Dist:{DistanceKm:F2}km | ETA:{EtaMinutes}min",
                    orderGroupId, attemptId, driverId, distanceKm, etaMinutes);
            }
            return Task.CompletedTask;
        }

        // ── Método 2 ──────────────────────────────────────────────────────
        public Task NotifyDriverAcceptedAsync(
            int orderGroupId,
            int driverId,
            int customerId,
            IEnumerable<int> merchantIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[STUB] DriverAccepted → OrderGroup:{OrderGroupId} | Driver:{DriverId} | " +
                "Customer:{CustomerId} | Merchants:[{MerchantIds}]",
                orderGroupId, driverId, customerId, string.Join(",", merchantIds));
            return Task.CompletedTask;
        }

        // ── Método 3 ──────────────────────────────────────────────────────
        public Task NotifyDriverRejectedAsync(
            int attemptId,
            int driverId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[STUB] DriverRejected → Attempt:{AttemptId} | Driver:{DriverId} | Reason:{Reason}",
                attemptId, driverId, reason ?? "N/A");
            return Task.CompletedTask;
        }

        // ── Método 4 ──────────────────────────────────────────────────────
        public Task NotifyOrderGroupStatusChangedAsync(
            int orderGroupId,
            string newStatus,
            int customerId,
            IEnumerable<int> merchantIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[STUB] OrderGroupStatusChanged → OrderGroup:{OrderGroupId} | " +
                "Status:{NewStatus} | Customer:{CustomerId} | Merchants:[{MerchantIds}]",
                orderGroupId, newStatus, customerId, string.Join(",", merchantIds));
            return Task.CompletedTask;
        }

        // ── Método 5 ──────────────────────────────────────────────────────
        public Task NotifyDriverArrivedAtStopAsync(
            int orderGroupId,
            int stopId,
            string stopType,
            int targetId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[STUB] DriverArrivedAtStop → OrderGroup:{OrderGroupId} | Stop:{StopId} | " +
                "Type:{StopType} | Target:{TargetId}",
                orderGroupId, stopId, stopType, targetId);
            return Task.CompletedTask;
        }

        // ── Método 6 ──────────────────────────────────────────────────────
        public Task NotifyDriverLocationUpdatedAsync(
            int driverId,
            int orderGroupId,
            decimal latitude,
            decimal longitude,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(
                "[STUB] DriverLocationUpdated → Driver:{DriverId} | " +
                "OrderGroup:{OrderGroupId} | Lat:{Lat} | Lon:{Lon}",
                driverId, orderGroupId, latitude, longitude);
            return Task.CompletedTask;
        }

        // ── Método 7 ──────────────────────────────────────────────────────
        public Task NotifyDispatchExpiredAsync(
            int attemptId,
            int orderGroupId,
            int roundNumber,
            CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(
                "[STUB] DispatchExpired → Attempt:{AttemptId} | " +
                "OrderGroup:{OrderGroupId} | Round:{RoundNumber}",
                attemptId, orderGroupId, roundNumber);
            return Task.CompletedTask;
        }

        public Task NotifyDriverDispatchOfferAsync(
    int driverId,
    int attemptId,
    int orderGroupId,
    CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[STUB] NotifyDriverDispatchOffer → Driver:{DriverId} | " +
                "Attempt:{AttemptId} | OrderGroup:{OrderGroupId}",
                driverId, attemptId, orderGroupId);
            return Task.CompletedTask;
        }
    }
}
