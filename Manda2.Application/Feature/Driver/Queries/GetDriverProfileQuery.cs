using Manda2.Application.Feature.Driver.Dtos;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Queries
{
    /// <summary>
    /// Retorna el perfil operativo de un driver.
    /// </summary>
    public class GetDriverProfileQuery : IQuery<DriverProfileDto>
    {
        public int DriverId { get; }

        public GetDriverProfileQuery(int driverId)
        {
            DriverId = driverId;
        }
    }
}
