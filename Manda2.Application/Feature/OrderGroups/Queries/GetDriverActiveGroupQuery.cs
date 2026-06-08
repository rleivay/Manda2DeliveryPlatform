// PROPÓSITO: Consulta el OrderGroup activo asignado a un Driver.
//            Usado por la app del repartidor al abrir sesión o refrescar pantalla.
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Application.Feature.OrderGroups.Dtos;
using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Queries
{
    /// <summary>
    /// Retorna el OrderGroup activo del driver (vía CurrentOrderGroupId).
    /// Si el driver no tiene grupo activo, retorna null.
    /// </summary>
    public class GetDriverActiveGroupQuery : IQuery<OrderGroupDetailDto?>
    {
        public int DriverId { get; }

        public GetDriverActiveGroupQuery(int driverId)
        {
            DriverId = driverId;
        }
    }
}
