// PROPÓSITO: Handler para registrar depósito de efectivo del driver.
//
// OPERACIONES ATÓMICAS:
//   1. Validar existencia del driver.
//   2. Validar que el monto sea positivo.
//   3. Crear registro CashDeposit en fin schema.
//   4. Reducir Driver.CurrentCashBalance en el monto depositado.
//      (No puede quedar negativo — mínimo 0).
//   5. Si nuevo balance < MaxCashLimit → desbloquear driver
//      (IsCashBlocked = false).
//   6. AuditLog con detalles del depósito.
//   7. Persistir en una sola transacción.
//
// INTEGRACIÓN SAP B1 (futura):
//   El CashDeposit es el origen del documento de pago en SAP B1.
//   El campo ReviewedBy y ReviewedAt preparan la trazabilidad del operador.
//

using Manda2.Application.Common;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Finance.Commands
{
    /// <summary>
    /// Handler para <see cref="CashDepositCommand"/>.
    /// </summary>
    public class CashDepositCommandHandler
        : ICommandHandler<CashDepositCommand, CashDepositResult>
    {
        private readonly IApplicationDbContext _db;

        public CashDepositCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CashDepositResult> HandleAsync(
            CashDepositCommand command, CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Validar monto positivo ────
            if (command.Amount <= 0)
                return Fail("El monto del depósito debe ser mayor a cero.");

            // ── PASO 2: Cargar driver con tracking ────
            var driver = await _db.Drivers
                .FirstOrDefaultAsync(d => d.Id == command.DriverId && !d.IsDeleted, ct);

            if (driver == null)
                return Fail($"Driver {command.DriverId} no encontrado.");

            // ── PASO 3: Crear registro CashDeposit ────
            // Status = "Confirmed" porque el operador lo registra en el momento
            // de recibir el efectivo físicamente.
            var deposit = new CashDeposit
            {
                DriverId = driver.Id,
                Amount = command.Amount,
                VoucherPath = command.VoucherPath,
                DepositDate = utcNow,
                Status = "Confirmed",
                ReviewedBy = command.ReceivedByUserId.ToString(),
                ReviewedAt = utcNow,
                ReviewNotes = command.ReviewNotes,
                CreatedAt = utcNow
            };

            _db.CashDeposits.Add(deposit);

            // ── PASO 4: Reducir balance del driver ────
            // El balance no puede quedar negativo (protección contra doble depósito).
            var previousBalance = driver.CurrentCashBalance;
            driver.CurrentCashBalance = Math.Max(0m, driver.CurrentCashBalance - command.Amount);

            // ── PASO 5: Desbloquear driver si el nuevo balance está bajo el límite ────
            bool driverUnblocked = false;

            if (driver.IsCashBlocked && driver.CurrentCashBalance < driver.MaxCashLimit)
            {
                driver.IsCashBlocked = false;
                driverUnblocked = true;
            }

            // ── PASO 6: AuditLog ────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(CashDeposit),
                EntityId = 0, // Se actualiza tras SaveChanges si es necesario
                Action = "CashDeposit",
                PerformedByUserId = command.ReceivedByUserId,
                Details = $"Driver {driver.Id} depositó {command.Amount:C}. " +
                                    $"Balance anterior: {previousBalance:C}. " +
                                    $"Balance nuevo: {driver.CurrentCashBalance:C}. " +
                                    $"Driver desbloqueado: {driverUnblocked}. " +
                                    $"Voucher: {command.VoucherPath ?? "N/A"}.",
                CreatedAt = utcNow
            });

            // ── PASO 7: Persistir en una sola transacción ────
            await _db.SaveChangesAsync(ct);

            return new CashDepositResult
            {
                Success = true,
                Message = driverUnblocked
                    ? $"Depósito registrado. Driver desbloqueado. Nuevo balance: {driver.CurrentCashBalance:C}."
                    : $"Depósito registrado. Nuevo balance: {driver.CurrentCashBalance:C}.",
                CashDepositId = deposit.Id,
                NewCashBalance = driver.CurrentCashBalance,
                DriverUnblocked = driverUnblocked
            };
        }

        private static CashDepositResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
