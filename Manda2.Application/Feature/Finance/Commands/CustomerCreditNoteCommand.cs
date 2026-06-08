// PROPÓSITO: Contrato de entrada para emitir una Nota de Crédito a un cliente.
//
// FLUJO:
//   BackOffice emite nota → Se crea CustomerCreditNote con RemainingAmount = TotalAmount
//   → AuditLog con trazabilidad SAP B1 (SAP_DocEntry / SAP_DocNum opcionales en MVP).
//
// INTEGRACIÓN SAP B1 (futura):
//   SAP_DocEntry y SAP_DocNum se populan cuando SAP B1 confirma el documento.
//   En MVP se dejan null y se completan en la fase de integración.
//

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Finance.Commands
{
    /// <summary>
    /// Comando para emitir una Nota de Crédito a un cliente.
    /// Solo BackOffice/Admin puede ejecutar este comando.
    /// </summary>
    public class CustomerCreditNoteCommand : ICommand<CustomerCreditNoteResult>
    {
        /// <summary>ID del cliente que recibirá la nota de crédito.</summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// ID del tipo de nota de crédito (FK a cfg.CreditNoteTypes).
        /// Valores seed: 1=Devolución, 2=Compensación, 3=GiftCard, 4=Ajuste manual.
        /// </summary>
        public int CreditNoteTypeId { get; set; }

        /// <summary>Monto total de la nota de crédito en moneda local (GTQ).</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Número de documento SAP B1 (opcional en MVP).
        /// Se completa cuando SAP B1 confirma el documento de crédito.
        /// Ej: "CN-2026-00123"
        /// </summary>
        public string? SAP_DocNum { get; set; }

        /// <summary>
        /// DocEntry de SAP B1 (opcional en MVP).
        /// Clave interna de SAP para el documento de crédito.
        /// </summary>
        public int? SAP_DocEntry { get; set; }

        /// <summary>
        /// ID del usuario BackOffice que emite la nota.
        /// Se guarda en AuditLog para trazabilidad.
        /// </summary>
        public int IssuedByUserId { get; set; }

        /// <summary>
        /// Motivo o contexto de la emisión (texto libre).
        /// Ej: "Cancelación de pedido #1234 por error del comercio."
        /// </summary>
        public string? Notes { get; set; }
    }
}
