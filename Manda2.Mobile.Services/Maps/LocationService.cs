using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Manda2.Mobile.Services.Maps
{
    /// <summary>
    /// Obtiene la ubicación GPS actual del dispositivo.
    /// Defensivo: nunca lanza excepción al caller.
    /// </summary>
    public class LocationService : ILocationService
    {
        public async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest(
                    GeolocationAccuracy.High,
                    TimeSpan.FromSeconds(10));

                var location = await Geolocation.Default.GetLocationAsync(request);

                return location != null
                    ? (location.Latitude, location.Longitude)
                    : (0, 0);
            }
            catch (FeatureNotSupportedException)
            {
                // GPS no soportado en esta plataforma
                return (0, 0);
            }
            catch (PermissionException)
            {
                // Usuario denegó permiso de ubicación
                return (0, 0);
            }
            catch
            {
                return (0, 0);
            }
        }
    }
}
