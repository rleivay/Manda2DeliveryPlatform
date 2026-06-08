// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/AcceptGroup/
//          AcceptGroupHandler.cs
//
// PROPÓSITO: Lógica de negocio al aceptar un grupo. Operaciones atómicas:
//
//   1. Validar que el grupo sigue en AwaitingDriverAssignment (no expiró)
//   2. Validar que el driver fue notificado y su respuesta sigue Pending
//   3. Validar que el driver no excede su límite de SubOrders activas
//      (configurable: DEFAULT_DRIVER_MAX_SUBORDER_LIMIT)
//   4. Marcar DispatchAttemptDriver.Response = Accepted
//   5. Cambiar OrderGroup.Status → DriverAccepted
//   6. Asignar OrderGroup.AssignedDriverId = DriverId
//   7. Rechazar automáticamente a los demás drivers notificados en este intento
//   8. Guardar y retornar la ruta completa
//
// PATRÓN: ICommandHandler<TCommand, TResult> del mediador propio.
// TRANSACCIÓN: Todo en un solo SaveChangesAsync → atómico.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Driver.Commands
{
    public class AcceptGroupDriverHandler
        : ICommandHandler<AcceptGroupDriverCommand, AcceptGroupDriverResult>
    {
        private readonly IApplicationDbContext _db;

        public AcceptGroupDriverHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AcceptGroupDriverResult> HandleAsync(
            AcceptGroupDriverCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar el grupo con todas las relaciones necesarias.
            //         Usamos tracking (sin AsNoTracking) porque vamos a
            //         modificar entidades en este mismo contexto.
            // ─────────────────────────────────────────────────────────────
            var group = await _db.OrderGroups
                .Include(og => og.Stops.OrderBy(s => s.Sequence))
                    .ThenInclude(s => s.Merchant)
                .Include(og => og.DispatchAttempts
                    .Where(da => da.Status == DispatchAttemptStatus.Sent))
                    .ThenInclude(da => da.NotifiedDrivers)
                .FirstOrDefaultAsync(og => og.Id == command.OrderGroupId, ct);

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Validaciones de negocio
            // ─────────────────────────────────────────────────────────────

            // 2a. El grupo debe existir
            if (group == null)
                return Fail("El grupo de órdenes no existe.");

            // 2b. El grupo debe seguir esperando asignación
            //     (otro driver pudo haberlo aceptado milisegundos antes)
            if (group.Status != OrderGroupStatus.AwaitingDriverAssignment)
                return Fail("Este pedido ya no está disponible.");

            // 2c. Encontrar la notificación específica de este driver
            var attempt = group.DispatchAttempts.FirstOrDefault();
            if (attempt == null)
                return Fail("No se encontró un intento de dispatch activo.");

            var driverNotification = attempt.NotifiedDrivers
                .FirstOrDefault(nd => nd.DriverId == command.DriverId);

            if (driverNotification == null)
                return Fail("Este pedido no fue asignado a tu cuenta.");

            // 2d. La respuesta debe seguir Pending (no expirada ni rechazada)
            if (driverNotification.Response != DriverDispatchResponse.Pending)
                return Fail("Tu tiempo para aceptar este pedido ha expirado.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar límite de SubOrders activas del driver
            //
            // Leer DEFAULT_DRIVER_MAX_SUBORDER_LIMIT desde AppConfig.
            // Si el driver ya tiene N grupos activos, no puede aceptar más.
            // "Activo" = DriverAccepted o InRoute
            // ─────────────────────────────────────────────────────────────
            var limitConfig = await _db.AppConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Key == "DEFAULT_DRIVER_MAX_SUBORDER_LIMIT", ct);

            var maxSubOrders = int.TryParse(limitConfig?.Value, out var lim) ? lim : 1;

            // Contar SubOrders activas del driver (en grupos que ya aceptó)
            var activeSubOrderCount = await _db.OrderGroups
                .AsNoTracking()
                .Where(og => og.DriverId == command.DriverId
                          && (og.Status == OrderGroupStatus.DriverAccepted
                           || og.Status == OrderGroupStatus.InRoute))
                .SumAsync(og => og.SubOrders.Count, ct);

            // Sumar las SubOrders del grupo que quiere aceptar
            var incomingSubOrders = await _db.SubOrders
                .AsNoTracking()
                .CountAsync(s => s.OrderGroupId == command.OrderGroupId, ct);

            if (activeSubOrderCount + incomingSubOrders > maxSubOrders)
                return Fail($"Ya tienes el máximo de {maxSubOrders} entregas activas permitidas.");

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Registrar la aceptación del driver
            // ─────────────────────────────────────────────────────────────
            driverNotification.Response = DriverDispatchResponse.Accepted;
            driverNotification.RespondedAtUtc = DateTime.UtcNow;

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Rechazar automáticamente a los demás drivers
            //         notificados en este mismo intento.
            //         Esto libera a los otros drivers para recibir
            //         nuevas ofertas de otros grupos.
            // ─────────────────────────────────────────────────────────────
            foreach (var otherDriver in attempt.NotifiedDrivers
                .Where(nd => nd.DriverId != command.DriverId
                          && nd.Response == DriverDispatchResponse.Pending))
            {
                otherDriver.Response = DriverDispatchResponse.Rejected;
                otherDriver.RespondedAtUtc = DateTime.UtcNow;
            }

            // Marcar el intento de dispatch como Accepted (cerrado)
            attempt.Status = DispatchAttemptStatus.Accepted;

            // ─────────────────────────────────────────────────────────────
            // PASO 6: Actualizar el OrderGroup
            //         - Asignar el driver
            //         - Cambiar estado a DriverAccepted
            //         - Registrar posición GPS del driver al momento de aceptar
            // ─────────────────────────────────────────────────────────────
            group.DriverId = command.DriverId;
            group.Status = OrderGroupStatus.DriverAccepted;
            group.DriverAcceptedAtUtc = DateTime.UtcNow;
            group.DriverLatAtAcceptance = command.CurrentLatitude;
            group.DriverLonAtAcceptance = command.CurrentLongitude;

            // ─────────────────────────────────────────────────────────────
            // PASO 7: Persistir todo en una sola transacción
            // ─────────────────────────────────────────────────────────────
            await _db.SaveChangesAsync(ct);

            // ─────────────────────────────────────────────────────────────
            // PASO 8: Construir la respuesta con la ruta completa
            //         para que la app del driver inicie navegación
            // ─────────────────────────────────────────────────────────────
            return new AcceptGroupDriverResult
            {
                Success = true,
                Message = "¡Pedido aceptado! Dirígete al primer punto de recogida.",
                TotalAmount = group.TotalAmount,
                PaymentMethodName = group.Payments
        .FirstOrDefault()?.PaymentMethod?.Name ?? "Efectivo",
                Stops = group.Stops.Select(s => new StopSummaryDto
                {
                    Sequence = s.Sequence,
                    StopType = s.StopType.ToString(),
                    Label = s.StopType == StopType.Pickup
                                    ? (s.Merchant?.Name ?? "Comercio")
                                    : "Entrega al cliente",
                    AddressText = s.AddressText,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                }).ToList()
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // HELPER: Fail — construye un resultado de error de forma limpia
        // ─────────────────────────────────────────────────────────────────
        private static AcceptGroupDriverResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
