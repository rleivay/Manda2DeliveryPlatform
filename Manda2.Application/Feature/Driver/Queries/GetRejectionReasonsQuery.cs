// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: Manda2.Application/Feature/Driver/Queries/GetRejectionReasons/
//          GetRejectionReasonsQuery.cs
//
// PROPÓSITO: Consulta el catálogo de motivos de rechazo activos.
//            La app MAUI llama a este endpoint al abrir la pantalla de rechazo
//            para poblar el Picker con los motivos disponibles.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Queries
{
    /// <summary>
    /// Consulta para obtener el catálogo de motivos de rechazo activos.
    /// No requiere parámetros: siempre retorna todos los motivos activos
    /// ordenados por DisplayOrder.
    /// </summary>
    public class GetRejectionReasonsQuery : IQuery<List<RejectionReasonDto>>
    {
    }
}
