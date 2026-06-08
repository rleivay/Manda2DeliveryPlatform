using Manda2.Contracts.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.CheckOut
{
    public interface IPaymentMethodApiService
    {
        /// <summary>
        /// Obtiene los métodos de pago activos habilitados para el checkout.
        /// </summary>
        Task<List<PaymentMethodDto>> GetActivePaymentMethodsAsync();
    }

    
}
