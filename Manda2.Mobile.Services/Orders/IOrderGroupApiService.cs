using Manda2.Contracts.CheckOut;
using Manda2.Mobile.Services.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Orders
{
    public interface IOrderGroupApiService
    {
        /// <summary>
        /// Envía el carrito en memoria al backend para crear el OrderGroup (Draft).
        /// </summary>
        Task<CreateOrderGroupResult> CreateOrderGroupAsync(
            IReadOnlyList<CartStateMerchant> merchants,
            CancellationToken ct = default);
    }
}
