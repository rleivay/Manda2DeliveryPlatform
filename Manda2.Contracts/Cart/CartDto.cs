using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Cart
{
    public class CartDto
    {
        /// <summary>ID del OrderGroup en estado Draft.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>Estado actual del grupo (debe ser Draft para edición).</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Dirección de entrega seleccionada por el cliente.</summary>
        public string DeliveryAddressText { get; set; } = string.Empty;

        // ─── FINANCIERO CONSOLIDADO ────────────────────────────────────────
        /// <summary>Suma de SubOrder.SubTotal de todos los comercios.</summary>
        public decimal Subtotal { get; set; }

        /// <summary>Cargo de delivery del OrderGroup (cfg.AppConfigs).</summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>Cargo de servicio de plataforma (cfg.AppConfigs).</summary>
        public decimal ServiceFee { get; set; }

        /// <summary>Total a cobrar al cliente = Subtotal + DeliveryFee + ServiceFee.</summary>
        public decimal Total { get; set; }

        // ─── COMERCIOS ────────────────────────────────────────────────────
        /// <summary>Un bloque por cada comercio incluido en el grupo.</summary>
        public List<CartMerchantDto> Merchants { get; set; } = new();
    }
}
