using Manda2.Contracts.CheckOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.CheckOut
{
    public interface ICheckoutApiService
    {
        /// <summary>
        /// Paso 1: Consolida logística y calcula delivery real.
        /// Transiciona de Draft a CapacityValidated.
        /// </summary>
        Task<ConfirmOrderGroupResult> ConfirmOrderGroupLogisticsAsync(ConfirmOrderGroupRequest request);

        /// <summary>
        /// Paso 2: Finaliza la orden aplicando el pago.
        /// Transiciona de CapacityValidated a PendingPayment o PaymentConfirmed.
        /// </summary>
        Task<ConfirmCheckoutResult> FinalizeCheckoutAsync(ConfirmCheckoutRequest request);
    }
}
