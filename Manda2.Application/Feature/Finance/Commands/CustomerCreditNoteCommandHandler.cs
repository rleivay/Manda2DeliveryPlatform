// PROPÓSITO: Handler para emitir una Nota de Crédito a un cliente.
//
// OPERACIONES ATÓMICAS:
//   1. Validar que el monto sea positivo.
//   2. Validar existencia del cliente (IsActive).
//   3. Validar existencia del CreditNoteType (IsActive).
//   4. Crear registro CustomerCreditNote:
//      - RemainingAmount = TotalAmount (saldo inicial = total).
//      - IsActive = true.
//      - SAP_DocEntry / SAP_DocNum opcionales (null en MVP).
//   5. AuditLog con trazabilidad completa para futura integración SAP B1.
//   6. Persistir en una sola transacción.
//
// INTEGRACIÓN SAP B1:
//   El AuditLog incluye SAP_DocNum y SAP_DocEntry para que el equipo de
//   integración pueda correlacionar el registro con el documento SAP.
//   En Fase 2: emitir evento hacia SAP B1 Service Layer aquí.
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
    /// Handler para <see cref="CustomerCreditNoteCommand"/>.
    /// </summary>
    public class CustomerCreditNoteCommandHandler
        : ICommandHandler<CustomerCreditNoteCommand, CustomerCreditNoteResult>
    {
        private readonly IApplicationDbContext _db;

        public CustomerCreditNoteCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CustomerCreditNoteResult> HandleAsync(
            CustomerCreditNoteCommand command, CancellationToken ct)
        {
            var utcNow = DateTime.UtcNow;

            // ── PASO 1: Validar monto positivo ────
            if (command.TotalAmount <= 0)
                return Fail("El monto de la nota de crédito debe ser mayor a cero.");

            // ── PASO 2: Validar existencia del cliente ────
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Id == command.CustomerId && !c.IsDeleted, ct);

            if (customer == null)
                return Fail($"Cliente {command.CustomerId} no encontrado.");

            if (!customer.IsActive)
                return Fail($"El cliente {command.CustomerId} está inactivo. " +
                            "No se puede emitir nota de crédito.");

            // ── PASO 3: Validar existencia del CreditNoteType ────
            var creditNoteType = await _db.CreditNoteTypes
                .FirstOrDefaultAsync(t => t.Id == command.CreditNoteTypeId && !t.IsDeleted, ct);

            if (creditNoteType == null)
                return Fail($"Tipo de nota de crédito {command.CreditNoteTypeId} no encontrado.");

            if (!creditNoteType.IsActive)
                return Fail($"El tipo de nota '{creditNoteType.Name}' está inactivo.");

            // ── PASO 4: Crear registro CustomerCreditNote ────
            // RemainingAmount = TotalAmount: el saldo inicial es el monto total.
            // Se reduce en cada aplicación de la nota a un pedido (Fase 2).
            var creditNote = new CustomerCreditNote
            {
                CustomerId = command.CustomerId,
                CreditNoteTypeId = command.CreditNoteTypeId,
                TotalAmount = command.TotalAmount,
                RemainingAmount = command.TotalAmount,   // Saldo inicial = total
                IsActive = true,
                SAP_DocEntry = command.SAP_DocEntry,
                SAP_DocNum = command.SAP_DocNum,
                CreatedAt = utcNow
            };

            _db.CustomerCreditNotes.Add(creditNote);

            // ── PASO 5: AuditLog con trazabilidad SAP B1 ────
            _db.AuditLogs.Add(new AuditLog
            {
                EntityName = nameof(CustomerCreditNote),
                EntityId = 0, // Se actualiza lógicamente tras SaveChanges
                Action = "IssueCustomerCreditNote",
                PerformedByUserId = command.IssuedByUserId,
                Details = $"Nota de crédito emitida. " +
                                    $"Cliente: {command.CustomerId}. " +
                                    $"Tipo: {creditNoteType.Name} (Id:{command.CreditNoteTypeId}). " +
                                    $"Monto: {command.TotalAmount:C}. " +
                                    $"SAP_DocNum: {command.SAP_DocNum ?? "N/A"}. " +
                                    $"SAP_DocEntry: {command.SAP_DocEntry?.ToString() ?? "N/A"}. " +
                                    $"Notas: {command.Notes ?? "Sin notas"}.",
                RequiresImmediateAttention = false,
                CreatedAt = utcNow
            });

            // ── PASO 6: Persistir en una sola transacción ────
            await _db.SaveChangesAsync(ct);

            return new CustomerCreditNoteResult
            {
                Success = true,
                Message = $"Nota de crédito emitida exitosamente por {command.TotalAmount:C}.",
                CreditNoteId = creditNote.Id,
                TotalAmount = creditNote.TotalAmount,
                CreditNoteTypeName = creditNoteType.Name
            };
        }

        private static CustomerCreditNoteResult Fail(string message) =>
            new() { Success = false, Message = message };
    }
}
