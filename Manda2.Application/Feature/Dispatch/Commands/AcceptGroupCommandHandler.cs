// FLUJO:
//   1. Cargar Driver + OrderGroup con DispatchAttempts.
//   2. Validar que existe un DispatchAttemptDriver activo para este driver
//      (la oferta fue enviada a él específicamente).
//   3. Validar disponibilidad del driver.
//   4. Validar estado del grupo.
//   5. Asignar: Driver → Busy, Group → DriverAccepted.
//   6. Neutralizar otros DispatchAttempts activos del grupo.
//   7. Registrar GPS de aceptación en el grupo.
//   8. AuditLog.
//
// DIFERENCIA CLAVE vs AcceptDispatchCommand:
//   El paso 2 (validar DispatchAttemptDriver) garantiza que solo el driver
//   al que se le envió la oferta puede aceptarla. El sistema no tiene
//   esta restricción (puede forzar asignación).
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    /// <summary>
    /// Handler del comando AcceptGroupCommand (driver-side).
    /// </summary>
    public class AcceptGroupCommandHandler : ICommandHandler<AcceptGroupCommand, AcceptGroupResult>
    {
        private readonly IApplicationDbContext _context;

        public AcceptGroupCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AcceptGroupResult> HandleAsync(
            AcceptGroupCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar entidades con tracking.
            // ─────────────────────────────────────────────────────────────
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == command.DriverId, ct);

            var group = await _context.OrderGroups
                .Include(g => g.DispatchAttempts)
                    .ThenInclude(a => a.NotifiedDrivers)
                .FirstOrDefaultAsync(g => g.Id == command.OrderGroupId, ct);

            if (driver == null || group == null)
                return new AcceptGroupResult(false, "Driver o Grupo no encontrados.", null);

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Validar que la oferta fue enviada a este driver.
            // Buscar un DispatchAttemptDriver Pending para este driver.
            // ─────────────────────────────────────────────────────────────
            var myOffer = group.DispatchAttempts
                .Where(a => a.Status == DispatchAttemptStatus.Sent)
                .SelectMany(a => a.NotifiedDrivers)
                .FirstOrDefault(d =>
                    d.DriverId == command.DriverId &&
                    d.Response == DriverDispatchResponse.Pending);

            if (myOffer == null)
                return new AcceptGroupResult(
                    false,
                    "No tienes una oferta activa para este pedido o ya expiró.",
                    null);

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar disponibilidad del driver.
            // ─────────────────────────────────────────────────────────────
            if (driver.Status != DriverStatus.Available || driver.CurrentOrderGroupId != null)
                return new AcceptGroupResult(
                    false,
                    "Ya tienes un pedido activo. No puedes aceptar otro.",
                    null);

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Validar estado del grupo.
            // ─────────────────────────────────────────────────────────────
            if (group.Status != OrderGroupStatus.AssignedToDriver &&
                group.Status != OrderGroupStatus.AwaitingDriverAssignment)
                return new AcceptGroupResult(
                    false,
                    "Este pedido ya no está disponible.",
                    null);

            var now = DateTime.UtcNow;

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Asignar driver al grupo.
            // ─────────────────────────────────────────────────────────────
            driver.Status = DriverStatus.Busy;
            driver.CurrentOrderGroupId = group.Id;

            group.Status = OrderGroupStatus.DriverAccepted;
            group.DriverId = driver.Id;
            group.DriverAcceptedAtUtc = now;
            // GPS de aceptación (campos a confirmar en entidad OrderGroup)
            group.DriverLatAtAcceptance = command.Latitude;
            group.DriverLonAtAcceptance = command.Longitude;
            group.UpdatedAt = now;

            // ─────────────────────────────────────────────────────────────
            // PASO 6: Marcar la oferta aceptada.
            // ─────────────────────────────────────────────────────────────
            myOffer.Response = DriverDispatchResponse.Accepted;
            myOffer.RespondedAtUtc = now;

            // ─────────────────────────────────────────────────────────────
            // PASO 7: Neutralizar todos los DispatchAttempts activos.
            //         Otros drivers con ofertas Pending → Expired.
            //         Attempts en Sent → CancelledByAssignment.
            // ─────────────────────────────────────────────────────────────
            foreach (var attempt in group.DispatchAttempts
                         .Where(a => a.Status == DispatchAttemptStatus.Sent))
            {
                attempt.Status = DispatchAttemptStatus.CancelledByAssignment;
                attempt.UpdatedAt = now;

                foreach (var otherDriver in attempt.NotifiedDrivers
                             .Where(d => d.DriverId != command.DriverId &&
                                         d.Response == DriverDispatchResponse.Pending))
                {
                    otherDriver.Response = DriverDispatchResponse.Expired;
                    otherDriver.RespondedAtUtc = now;
                }
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 8: AuditLog inmutable.
            // ─────────────────────────────────────────────────────────────
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "DriverAcceptedGroup",
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                PerformedByUserId = driver.Id,
                CreatedAt = now,
                Details = $"Driver {driver.Id} aceptó el grupo {group.Id} " +
                                       $"desde ({command.Latitude},{command.Longitude})."
            });

            await _context.SaveChangesAsync(ct);

            return new AcceptGroupResult(true, "Pedido aceptado correctamente.", group.Id);
        }
    }
}
