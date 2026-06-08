using Manda2.Application.Common;
using Manda2.Application.Common.Exceptions;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Catalog.Commands.UpdateOperatingSchedule
{
    public class UpdateOperatingScheduleHandler : ICommandHandler<UpdateOperatingScheduleCommand, string>
    {
        private readonly IApplicationDbContext _db;

        public UpdateOperatingScheduleHandler(IApplicationDbContext db) => _db = db;

        public async Task<string> HandleAsync(UpdateOperatingScheduleCommand command, CancellationToken ct)
        {
            // 1. Validar existencia del actor según tipo
            if (command.ActorType == ActorType.Merchant)
            {
                if (!await _db.Merchants.AnyAsync(m => m.Id == command.ActorId, ct))
                    throw new BusinessRuleException($"Comercio con Id {command.ActorId} no encontrado.");
            }
            else if (command.ActorType == ActorType.Driver)
            {
                if (!await _db.Drivers.AnyAsync(d => d.Id == command.ActorId, ct))
                    throw new BusinessRuleException($"Repartidor con Id {command.ActorId} no encontrado.");
            }

            // 2. Obtener horarios actuales para limpiar/actualizar
            var existingSchedules = await _db.OperatingSchedules
                .Where(s => s.ActorType == command.ActorType &&
                           (command.ActorType == ActorType.Merchant ? s.MerchantId == command.ActorId : s.DriverId == command.ActorId))
                .ToListAsync(ct);

            // 3. Procesar días
            foreach (var dayDto in command.Days)
            {
                var schedule = existingSchedules.FirstOrDefault(s => s.DayOfWeek == dayDto.DayOfWeek);

                if (schedule == null)
                {
                    schedule = new OperatingSchedule
                    {
                        ActorType = command.ActorType,
                        MerchantId = command.ActorType == ActorType.Merchant ? command.ActorId : null,
                        DriverId = command.ActorType == ActorType.Driver ? command.ActorId : null,
                        DayOfWeek = dayDto.DayOfWeek
                    };
                    _db.OperatingSchedules.Add(schedule);
                }

                schedule.OpenTime = dayDto.OpenTime;
                schedule.CloseTime = dayDto.CloseTime;
                schedule.IsClosed = dayDto.IsClosed;
                schedule.ClosureReasonId = dayDto.ClosureReasonId;
            }

            await _db.SaveChangesAsync(ct);
            return $"Horario de {command.ActorType} actualizado exitosamente.";
        }
    }
}
