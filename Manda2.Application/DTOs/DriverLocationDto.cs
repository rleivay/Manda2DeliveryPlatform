using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.DTOs
{
    /// <summary>
    /// DTO de posición GPS del driver.
    /// Viaja por SignalR hacia cliente, comercio y BackOffice.
    /// </summary>
    public record DriverLocationDto
    {
        public int DriverId { get; init; }
        public decimal Latitude { get; init; }
        public decimal Longitude { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
