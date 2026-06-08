// PROPÓSITO: Handler para actualizar GPS del driver.
//
// OPERACIONES:
//   1. Validar existencia y estado activo del driver.
//   2. Validar rango de coordenadas (lat: -90/90, lon: -180/180).
//   3. Persistir LastLatitude, LastLongitude, LastLocationUpdateAt.
//   4. Si el driver tiene un OrderGroup activo → notificar al cliente
//      vía INotificationService.NotifyDriverLocationUpdatedAsync.
//
// NOTA ARQUITECTÓNICA:
//   No se registra AuditLog aquí — alta frecuencia (cada 5-10s).
//   El AuditLog de GPS se reserva para eventos críticos (aceptación, entrega).
//   En Fase 2: reemplazar INotificationService por SignalR Hub directo
//   para reducir latencia de broadcasting.

using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Handler para <see cref="UpdateDriverLocationCommand"/>.
    /// </summary>
    public class UpdateDriverLocationCommandHandler
        : ICommandHandler<UpdateDriverLocationCommand, UpdateDriverLocationResult>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public UpdateDriverLocationCommandHandler(
            IApplicationDbContext context,
            INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<UpdateDriverLocationResult> HandleAsync(
            UpdateDriverLocationCommand command, CancellationToken ct)
        {
            // ── PASO 1: Validar rango de coordenadas ────
            // Coordenadas fuera de rango indican error del cliente GPS o ataque.
            if (command.Latitude < -90m || command.Latitude > 90m)
                return Fail("Latitud fuera de rango válido (-90 a 90).");

            if (command.Longitude < -180m || command.Longitude > 180m)
                return Fail("Longitud fuera de rango válido (-180 a 180).");

            // ── PASO 2: Cargar driver con tracking ────
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == command.DriverId && !d.IsDeleted, ct);

            if (driver == null)
                return Fail("Driver no encontrado.");

            if (!driver.IsActive)
                return Fail("Driver inactivo. No se puede actualizar ubicación.");

            // ── PASO 3: Actualizar campos GPS ────
            driver.LastLatitude = command.Latitude;
            driver.LastLongitude = command.Longitude;
            driver.LastLocationUpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            // ── PASO 4: Notificar al cliente si el driver tiene un grupo activo ────
            // Solo notificamos si hay un OrderGroup asignado — evita broadcasts innecesarios
            // cuando el driver está en estado Available (sin pedido activo).
            if (driver.CurrentOrderGroupId.HasValue)
            {
                // Obtener CustomerId del OrderGroup para dirigir la notificación
                var customerId = await _context.OrderGroups
                    .AsNoTracking()
                    .Where(og => og.Id == driver.CurrentOrderGroupId.Value)
                    .Select(og => og.CustomerId)
                    .FirstOrDefaultAsync(ct);

                if (customerId > 0)
                {
                    // Fire-and-forget: si la notificación falla, no revertimos el GPS.
                    // El estado en BD ya es correcto — la notificación es best-effort.
                    await _notifications.NotifyDriverLocationUpdatedAsync(
                        driverId: driver.Id,
                        orderGroupId: driver.CurrentOrderGroupId.Value,
                        latitude: command.Latitude,
                        longitude: command.Longitude,
                        cancellationToken: ct);
                }
            }

            return new UpdateDriverLocationResult
            {
                Success = true,
                LastUpdate = driver.LastLocationUpdateAt!.Value
            };
        }

        /// <summary>Helper para retornar errores de forma consistente.</summary>
        private static UpdateDriverLocationResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
