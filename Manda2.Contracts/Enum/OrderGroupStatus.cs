using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Enum
{
    public enum OrderGroupStatus
    {
        Draft = 1,                 // Carrito activo (no pagado)
        CapacityValidated = 2,     // Paso previo a checkout
        PendingPayment = 3,        // Esperando confirmación de pasarela
        PaymentConfirmed = 4,      // Listo para flujo operativo
        AwaitingMerchantAcceptance = 5, //Pagado Esperando que el comercio acepte o rechace
        AwaitingDriverAssignment = 6, // Comercio aceptó, pendiente de asignación automática o manual de driver
        AssignedToDriver = 7,      // Driver notificado, pendiente de aceptar
        DriverAccepted = 8,        // Driver aceptó, en camino a pickups
        InRoute = 9,               // Ya recolectó y va hacia el cliente
        Delivered = 10, // Entregado al cliente
        Cancelled = 11, // Pedido cancelado (en cualquier etapa previa a Delivered)
        Scheduled = 12,          // Pedido para fecha futura
        AwaitingManualAssignment=13, // Para casos donde se asigna manualmente un driver
    }
}
