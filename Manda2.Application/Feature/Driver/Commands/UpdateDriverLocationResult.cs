using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Resultado del comando UpdateDriverLocationCommand.
    /// </summary>
    public class UpdateDriverLocationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        /// <summary>Timestamp UTC del último update persistido.</summary>
        public DateTime LastUpdate { get; set; }
    }
}
