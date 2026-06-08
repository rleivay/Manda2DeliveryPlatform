using Manda2.Application.Feature.OrderGroups.Dtos;
using Manda2.Application.Mediator;
using Manda2.Contracts.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Commands
{
    public class CreateOrderGroupCommand : ICommand<CreateOrderGroupResult>
    {
        public int CustomerId { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lon { get; set; }
        public string? Notes { get; set; }

        // El carrito: Lista de comercios, cada uno con sus productos
        public List<MerchantCartDto> Merchants { get; set; } = new();
    }
}
