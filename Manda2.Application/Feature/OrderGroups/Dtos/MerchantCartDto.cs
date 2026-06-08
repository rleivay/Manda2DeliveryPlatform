using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.OrderGroups.Dtos
{
    public class MerchantCartDto
    {
        public int MerchantId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
