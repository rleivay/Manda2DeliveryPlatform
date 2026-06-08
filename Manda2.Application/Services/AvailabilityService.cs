using Manda2.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Services
{
    public class AvailabilityService
    {
        private readonly IConfiguration _config;

        public AvailabilityService(IConfiguration config) => _config = config;

        // ─── EVALUADOR EN MEMORIA (para un solo actor ya cargado) ───────────
        public bool IsCurrentlyAvailable(Merchant merchant)
        {
            if (!merchant.IsActive || !merchant.IsApproved || merchant.IsDeleted) return false;
            if (!merchant.IsOnline) return false;

            var (today, nowTime, dayOfWeek) = GetLocalTime();

            var exception = merchant.OperatingScheduleExceptions
                .FirstOrDefault(e => e.Date == today);

            if (exception != null)
                return EvaluateException(exception, nowTime);

            var baseSchedule = merchant.OperatingSchedules
                .FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

            if (baseSchedule == null || baseSchedule.IsClosed) return false;

            return nowTime >= baseSchedule.OpenTime && nowTime <= baseSchedule.CloseTime;
        }

        // ─── EXPRESIÓN SQL (para filtrar en DB sin cargar en memoria) ───────
        public Expression<Func<Merchant, bool>> IsAvailableExpression()
        {
            var (today, nowTime, dayOfWeek) = GetLocalTime();

            return m =>
                m.IsActive &&
                m.IsApproved &&
                !m.IsDeleted &&
                m.IsOnline &&
                (
                    // ── CASO 1: Existe excepción HOY ──────────────────────────
                    m.OperatingScheduleExceptions.Any(e => e.Date == today)
                    ?
                    // Existe excepción → evaluar tipo
                    m.OperatingScheduleExceptions.Any(e =>
                        e.Date == today &&
                        !e.IsClosed &&
                        (
                            // Horario especial completo (OpenTime y CloseTime)
                            (e.OpenTime != null && e.CloseTime != null &&
                             nowTime >= e.OpenTime.Value && nowTime <= e.CloseTime.Value)
                            ||
                            // Extensión de cierre (solo CloseTime, sin OpenTime)
                            (e.OpenTime == null && e.CloseTime != null &&
                             nowTime <= e.CloseTime.Value)
                        )
                    )
                    :
                    // ── CASO 2: Sin excepción → evaluar horario base ──────────
                    m.OperatingSchedules.Any(s =>
                        s.DayOfWeek == dayOfWeek &&
                        !s.IsClosed &&
                        nowTime >= s.OpenTime &&
                        nowTime <= s.CloseTime
                    )
                );
        }

        // ─── HELPER: Obtener tiempo local ────────────────────────────────────
        public (DateOnly today, TimeSpan nowTime, DayOfWeek dayOfWeek) GetLocalTime()
        {
            var tzId = _config["APP_TIMEZONE"] ?? "America/Managua";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            return (
                DateOnly.FromDateTime(localNow),
                localNow.TimeOfDay,
                localNow.DayOfWeek
            );
        }

        // ─── HELPER: Evaluar excepción en memoria ────────────────────────────
        private static bool EvaluateException(OperatingScheduleException exception, TimeSpan nowTime)
        {
            if (exception.IsClosed) return false;

            // Horario especial completo
            if (exception.OpenTime.HasValue && exception.CloseTime.HasValue)
                return nowTime >= exception.OpenTime.Value && nowTime <= exception.CloseTime.Value;

            // Extensión de cierre
            if (!exception.OpenTime.HasValue && exception.CloseTime.HasValue)
                return nowTime <= exception.CloseTime.Value;

            return false;
        }

        ///Driver---
        ///
        public bool IsCurrentlyAvailable(Driver driver)
        {
            // 1. BackOffice control
            if (!driver.IsActive) return false;
            if (!driver.IsCashBlocked) return false;

            // 2. Switch manual
            if (!driver.IsOnline) return false;

            // 3. Tiempo local
            var (today, nowTime, dayOfWeek) = GetLocalTime();

            // 4. Excepciones
            var exception = driver.OperatingScheduleExceptions
                .FirstOrDefault(e => e.Date == today);

            if (exception != null)
                return EvaluateException(exception, nowTime);

            // 5. Horario base
            var baseSchedule = driver.OperatingSchedules
                .FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

            if (baseSchedule == null || baseSchedule.IsClosed) return false;

            return nowTime >= baseSchedule.OpenTime && nowTime <= baseSchedule.CloseTime;
        }

        public Expression<Func<Driver, bool>> IsDriverAvailableExpression()
        {
            var (today, nowTime, dayOfWeek) = GetLocalTime();

            return d =>
                d.IsActive &&
                d.IsOnline &&
                (
                    // Excepción HOY
                    d.OperatingScheduleExceptions.Any(e => e.Date == today)
                    ?
                    d.OperatingScheduleExceptions.Any(e =>
                        e.Date == today &&
                        !e.IsClosed &&
                        (
                            (e.OpenTime != null && e.CloseTime != null &&
                             nowTime >= e.OpenTime.Value && nowTime <= e.CloseTime.Value)
                            ||
                            (e.OpenTime == null && e.CloseTime != null &&
                             nowTime <= e.CloseTime.Value)
                        )
                    )
                    :
                    // Horario base
                    d.OperatingSchedules.Any(s =>
                        s.DayOfWeek == dayOfWeek &&
                        !s.IsClosed &&
                        nowTime >= s.OpenTime &&
                        nowTime <= s.CloseTime
                    )
                );
        }
    }
}
