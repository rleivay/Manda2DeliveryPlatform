using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Merchant.Commands
{
    public class MarkSubOrderReadyCommand : ICommand<MarkSubOrderReadyResult>
    {
        /// <summary>ID de la SubOrder lista para recoger.</summary>
        public int SubOrderId { get; set; }

        /// <summary>ID del Merchant que marca como lista.</summary>
        public int MerchantId { get; set; }
    }
}
