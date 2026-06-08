// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Commands/ArriveAtStop/
//          ArriveAtStopHandler.cs
//
// PROPÓSITO: Lógica de negocio al registrar la llegada del driver a una parada.
//
// FLUJO:
//   1. Validar que la parada existe y pertenece a un grupo asignado al driver.
//   2. Validar que la parada no fue completada previamente.
//   3. Validar que la parada anterior en la secuencia ya fue completada
//      (no puede saltarse paradas).
//   4. Registrar ArrivedAt con timestamp UTC.
//   5. Persistir y retornar tipo de parada para que la app sepa qué mostrar.
//
// REGLA DE NEGOCIO CLAVE:
//   No se puede marcar llegada a una parada si la parada anterior
//   (Sequence - 1) no tiene IsCompleted = true.
//   Excepción: la primera parada (Sequence = 1) no tiene prerequisito.
//
// NOTA SAP B1 (Sprint 5):
//   La llegada al Dropoff (cliente) disparará la notificación de entrega
//   que SAP B1 necesita para cerrar el documento de despacho.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Microsoft.EntityFrameworkCore;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Driver.Commands.ArriveAtStop
{
    /// <summary>
    /// Handler del comando ArriveAtStopCommand.
    /// </summary>
    public class ArriveAtStopHandler
        : ICommandHandler<ArriveAtStopCommand, ArriveAtStopResult>
    {
        private readonly IApplicationDbContext _db;

        public ArriveAtStopHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ArriveAtStopResult> HandleAsync(
            ArriveAtStopCommand command, CancellationToken ct)
        {
            // ─────────────────────────────────────────────────────────────
            // PASO 1: Cargar la parada con su grupo y todas las paradas
            //         del grupo para validar secuencia.
            // ─────────────────────────────────────────────────────────────
            var stop = await _db.OrderGroupStops
                .Include(s => s.OrderGroup)
                    .ThenInclude(og => og.Stops)
                .FirstOrDefaultAsync(s => s.Id == command.StopId, ct);

            if (stop == null)
                return Fail("La parada no existe.");

            // ─────────────────────────────────────────────────────────────
            // PASO 2: Validar que el grupo pertenece al driver que ejecuta.
            //         Evita que un driver marque paradas de otro.
            // ─────────────────────────────────────────────────────────────
            if (stop.OrderGroup.DriverId != command.DriverId)
                return Fail("Esta parada no pertenece a tu ruta activa.");

            // ─────────────────────────────────────────────────────────────
            // PASO 3: Validar que la parada no fue completada previamente.
            // ─────────────────────────────────────────────────────────────
            if (stop.IsCompleted)
                return Fail("Esta parada ya fue completada.");

            // ─────────────────────────────────────────────────────────────
            // PASO 4: Validar que ya llegó (ArrivedAt) no fue marcado antes.
            //         Evita doble tap en la app.
            // ─────────────────────────────────────────────────────────────
            if (stop.ArrivedAt.HasValue)
                return Fail("La llegada a esta parada ya fue registrada.");

            // ─────────────────────────────────────────────────────────────
            // PASO 5: Validar secuencia — la parada anterior debe estar
            //         completada antes de poder marcar llegada a esta.
            //         La primera parada (Sequence = 1) no tiene prerequisito.
            // ─────────────────────────────────────────────────────────────
            if (stop.Sequence > 1)
            {
                var previousStop = stop.OrderGroup.Stops
                    .FirstOrDefault(s => s.Sequence == stop.Sequence - 1);

                if (previousStop != null && !previousStop.IsCompleted)
                    return Fail("Debes completar la parada anterior antes de continuar.");
            }

            // ─────────────────────────────────────────────────────────────
            // PASO 6: Registrar llegada.
            // ─────────────────────────────────────────────────────────────
            stop.ArrivedAt = DateTime.UtcNow;

            // ─────────────────────────────────────────────────────────────
            // FUTURO Sprint 4 — Validación de proximidad GPS:
            //   Calcular distancia entre command.CurrentLatitude/Longitude
            //   y stop.Latitude/Longitude. Si > umbral configurable (ej: 200m),
            //   rechazar o registrar alerta de fraude.
            // ─────────────────────────────────────────────────────────────

            await _db.SaveChangesAsync(ct);

            return new ArriveAtStopResult
            {
                Success = true,
                Message = stop.StopType == StopType.Pickup
                    ? "Llegada al comercio registrada. Confirma cuando tengas el pedido."
                    : "Llegada al cliente registrada. Confirma cuando entregues el pedido.",
                StopType = stop.StopType,
                ArrivedAtUtc = stop.ArrivedAt!.Value
            };
        }

        /// <summary>
        /// Helper privado para retornar resultados de error de forma consistente.
        /// </summary>
        private ArriveAtStopResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}