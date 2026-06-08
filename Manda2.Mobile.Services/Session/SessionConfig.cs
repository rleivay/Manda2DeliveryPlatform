using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Session
{
    /// <summary>
    /// Configuraciones de negocio cargadas desde el BackEnd al iniciar sesión.
    /// Viven en memoria durante la sesión activa. No se persisten en SecureStorage.
    /// </summary>
    public class SessionConfig
    {
        /// <summary>
        /// Máximo de comercios distintos permitidos en un mismo carrito.
        /// Viene de cfg.AppConfigs → "DEFAULT_DRIVER_MAX_SUBORDER_LIMIT"
        /// </summary>
        public int MerchantLimit { get; init; } = 1;

        // Aquí irán más configuraciones de negocio en el futuro:
        // public decimal DeliveryFeeBase { get; init; }
        // public bool IsMarketEnabled { get; init; }
    }
}
