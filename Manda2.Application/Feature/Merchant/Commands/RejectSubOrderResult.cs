using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Commands
{
    public class RejectSubOrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;

        /// <summary>
        /// True si el OrderGroup fue cancelado como consecuencia del rechazo.
        /// </summary>
        public bool OrderGroupCancelled { get; set; }
    }
}
