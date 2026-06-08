using Microsoft.Maui.Devices.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Mobile.Services.Maps
{
    /// <summary>
    /// Servicio de geolocalización del dispositivo.
    /// Responsabilidad única: obtener lat/lon actuales.
    /// La conversión a dirección textual es responsabilidad del backend.
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Obtiene la ubicación GPS actual del dispositivo.
        /// Retorna (0, 0) si no se puede obtener la ubicación.
        /// </summary>
        Task<(double Latitude, double Longitude)> GetCurrentLocationAsync();
    }
}
