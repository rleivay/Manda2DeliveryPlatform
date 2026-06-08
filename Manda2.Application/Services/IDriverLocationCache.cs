// PROPÓSITO: Abstracción del caché GPS de drivers.
//            Implementación 1 (MVP): InMemoryDriverLocationCache.
//            Implementación 2 (Producción): RedisDriverLocationCache.
//
// PRINCIPIO: El Hub y los Handlers dependen de esta interfaz,
//            no de la implementación concreta.
// ═══════════════════════════════════════════════════════════════════════════

using Manda2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Services
{
    /// <summary>
    /// Contrato para el caché de posiciones GPS de drivers.
    /// </summary>
    public interface IDriverLocationCache
    {
        /// <summary>Guarda o actualiza la posición del driver.</summary>
        Task SetAsync(int driverId, DriverLocationDto location);

        /// <summary>Obtiene la última posición conocida. Null si no existe.</summary>
        Task<DriverLocationDto?> GetAsync(int driverId);

        /// <summary>Elimina la posición al desconectarse el driver.</summary>
        Task RemoveAsync(int driverId);

        /// <summary>
        /// Obtiene todas las posiciones activas.
        /// Usado por BackOffice para el mapa de operaciones.
        /// </summary>
        Task<IEnumerable<DriverLocationDto>> GetAllAsync();
    }

    
}
