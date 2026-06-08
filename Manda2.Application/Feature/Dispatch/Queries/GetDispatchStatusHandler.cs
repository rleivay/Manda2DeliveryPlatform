// PROPÓSITO: Handler de solo lectura para GetDispatchStatusQuery.
//            Proyecta DispatchAttempts + DispatchAttemptDrivers a DTO.
//            AsNoTracking — solo lectura, máximo performance.
//

using Manda2.Application.Common;
using Manda2.Application.Feature.Dispatch.Dtos;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Queries
{
    /// <summary>
    /// Handler para <see cref="GetDispatchStatusQuery"/>.
    /// </summary>
    public class GetDispatchStatusHandler : IQueryHandler<GetDispatchStatusQuery, DispatchStatusDto>
    {
        private readonly IApplicationDbContext _db;

        public GetDispatchStatusHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DispatchStatusDto> HandleAsync(
            GetDispatchStatusQuery query, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            // ── PASO 1: Verificar que el OrderGroup existe ────
            var group = await _db.OrderGroups
                .AsNoTracking()
                .Where(og => og.Id == query.OrderGroupId)
                .Select(og => new { og.Id, og.Status, og.DriverId })
                .FirstOrDefaultAsync(ct);

            if (group == null)
                throw new KeyNotFoundException(
                    $"OrderGroup {query.OrderGroupId} no encontrado.");

            // ── PASO 2: Cargar todos los intentos con sus drivers notificados ────
            var attempts = await _db.DispatchAttempts
                .AsNoTracking()
                .Include(a => a.NotifiedDrivers)
                .Where(a => a.OrderGroupId == query.OrderGroupId && !a.IsDeleted)
                .OrderBy(a => a.RoundNumber)
                .ToListAsync(ct);

            // ── PASO 3: Identificar ronda activa (Status = Sent) ────
            var activeAttempt = attempts
                .FirstOrDefault(a => a.Status == DispatchAttemptStatus.Sent);

            // ── PASO 4: Construir DTO de ronda activa ────
            ActiveAttemptDto? activeDto = null;

            if (activeAttempt != null)
            {
                var secondsRemaining = (int)(activeAttempt.ExpiresAt - now).TotalSeconds;

                activeDto = new ActiveAttemptDto
                {
                    AttemptId = activeAttempt.Id,
                    RoundNumber = activeAttempt.RoundNumber,
                    StartedAt = activeAttempt.StartedAt,
                    ExpiresAt = activeAttempt.ExpiresAt,
                    SecondsRemaining = secondsRemaining > 0 ? secondsRemaining : 0,

                    // Proyectar cada DispatchAttemptDriver con campos reales
                    NotifiedDrivers = activeAttempt.NotifiedDrivers
                        .Select(nd => new DriverOfferStatusDto
                        {
                            DriverId = nd.DriverId,
                            Response = nd.Response.ToString(),
                            NotifiedAtUtc = nd.NotifiedAtUtc,
                            RespondedAtUtc = nd.RespondedAtUtc,
                            DistanceKm = nd.DistanceKm,
                            EstimatedArrivalMinutes = nd.EstimatedArrivalMinutes
                        }).ToList()
                };
            }

            // ── PASO 5: Construir historial de rondas ────
            var history = attempts
                .Where(a => a.Status != DispatchAttemptStatus.Sent)
                .Select(a => new AttemptSummaryDto
                {
                    AttemptId = a.Id,
                    RoundNumber = a.RoundNumber,
                    Status = a.Status.ToString(),
                    StartedAt = a.StartedAt,
                    ExpiresAt = a.ExpiresAt,
                    NotifiedCount = a.NotifiedDrivers.Count,
                    RejectedCount = a.NotifiedDrivers
                        .Count(nd => nd.Response == DriverDispatchResponse.Rejected)
                }).ToList();

            // ── PASO 6: Ensamblar respuesta final ────
            return new DispatchStatusDto
            {
                OrderGroupId = group.Id,
                OrderGroupStatus = group.Status.ToString(),
                AssignedDriverId = group.DriverId,
                ActiveAttempt = activeDto,
                AttemptHistory = history
            };
        }
    }
}
