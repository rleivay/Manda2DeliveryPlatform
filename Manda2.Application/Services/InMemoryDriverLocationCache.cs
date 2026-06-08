using Manda2.Application.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Services
{
    /// <summary>
    /// Caché GPS en memoria. Singleton — una instancia por proceso.
    /// Thread-safe gracias a ConcurrentDictionary.
    /// </summary>
    public class InMemoryDriverLocationCache : IDriverLocationCache
    {
        // Key: DriverId | Value: última posición conocida
        private readonly ConcurrentDictionary<int, DriverLocationDto> _cache = new();

        public Task SetAsync(int driverId, DriverLocationDto location)
        {
            _cache[driverId] = location;
            return Task.CompletedTask;
        }

        public Task<DriverLocationDto?> GetAsync(int driverId)
        {
            _cache.TryGetValue(driverId, out var location);
            return Task.FromResult(location);
        }

        public Task RemoveAsync(int driverId)
        {
            _cache.TryRemove(driverId, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<DriverLocationDto>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<DriverLocationDto>>(_cache.Values);
        }
    }
}
