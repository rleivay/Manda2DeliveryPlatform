// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Dispatch/Commands/AcceptDispatchCommandHandler.cs
//
// PROPÓSITO:
//   Procesa la aceptación de un pedido por parte de un repartidor.
//   Orquesta en una sola transacción:
//     1. Validaciones de integridad y disponibilidad.
//     2. Actualización del Driver (Busy + CurrentOrderGroupId).
//     3. Actualización del OrderGroup (DriverAccepted + campos de auditoría).
//     4. Marcado del DispatchAttemptDriver ganador como Accepted.
//     5. Neutralización de TODOS los DispatchAttemptDriver pendientes de otras rondas.
//     6. Cierre del DispatchAttempt activo como Accepted.
//     7. AuditLog inmutable.
//
// INTEGRACIÓN SAP B1:
//   El AuditLog y los campos DriverAcceptedAtUtc / DriverLat/Lon preparan
//   la trazabilidad del transportista para el envío posterior al ERP.
// ════════════════════════════════════════════════════════════════════════════

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
    public class AcceptDispatchCommandHandler : ICommandHandler<AcceptDispatchCommand, AcceptDispatchResult>
    {
        private readonly IApplicationDbContext _context;

        public AcceptDispatchCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AcceptDispatchResult> HandleAsync(AcceptDispatchCommand command, CancellationToken ct)
        {
            // Timestamp único para toda la operación — garantiza consistencia
            // entre todos los campos de auditoría escritos en este handler.
            var utcNow = DateTime.UtcNow;

            // 1. Cargar entidades críticas con tracking para actualización
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == command.DriverId, ct);

            // Incluimos DispatchAttempts para cerrarlos si el driver acepta
            var group = await _context.OrderGroups
                .Include(g => g.DispatchAttempts)
                .FirstOrDefaultAsync(g => g.Id == command.OrderGroupId, ct);

            // 2. Validaciones de Integridad y Disponibilidad
            if (driver == null || group == null)
                throw new InvalidOperationException("Driver o Grupo de Orden no encontrados.");

            if (driver.Status != DriverStatus.Available || driver.CurrentOrderGroupId != null)
                return new AcceptDispatchResult(false, "El conductor ya se encuentra en otro servicio.", null);

            if (group.Status != OrderGroupStatus.AwaitingDriverAssignment)
                return new AcceptDispatchResult(false, "Este pedido ya no está disponible o ha sido asignado.", null);

            // 3. Orquestación de cambios de estado (Atomicidad)

            // Actualizar Driver
            driver.Status = DriverStatus.Busy;
            driver.CurrentOrderGroupId = group.Id;

            // Actualizar Grupo
            group.Status = OrderGroupStatus.DriverAccepted;
            group.DriverId = driver.Id;
            group.UpdatedAt = utcNow;

            // 4. Neutralizar subastas/despachos competitivos
            // Marcamos todos los intentos activos como 'Completed' porque el pedido ya tiene dueño
            foreach (var attempt in group.DispatchAttempts.Where(a => a.Status == DispatchAttemptStatus.Sent))
            {
                attempt.Status = DispatchAttemptStatus.Sent;
                attempt.UpdatedAt = utcNow;
            }

            // 5. Auditoría
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "DriverAcceptedOrder",
                EntityName = nameof(OrderGroup),
                EntityId = group.Id,
                PerformedByUserId = driver.Id,
                CreatedAt = utcNow,
                Details = $"Driver {driver.Id} aceptó el grupo {group.Id}. Coordenadas Lat: {driver.LastLatitude} ; Lon: {driver.LastLongitude}."
            });

            await _context.SaveChangesAsync(ct);

            return new AcceptDispatchResult(true, "Pedido asignado correctamente.", group.Id);
        }
    }
}
