// PROPÓSITO: Contrato de entrada para actualizar la posición GPS del driver.
//            Alta frecuencia: cada 5-10 segundos mientras está en ruta.
//            Speed y Heading son opcionales — útiles para proyecciones de ETA
//            y para rotar el ícono del driver en el mapa del cliente.

using Manda2.Application.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Driver.Commands
{
    /// <summary>
    /// Comando para actualizar la posición GPS del driver autenticado.
    /// DriverId se extrae del JWT en el controller — no se acepta en el body.
    /// </summary>
    public class UpdateDriverLocationCommand : ICommand<UpdateDriverLocationResult>
    {
        /// <summary>ID del driver. Asignado por el controller desde el JWT claim "did".</summary>
        public int DriverId { get; set; }

        /// <summary>Latitud GPS actual. Rango válido: -90 a 90.</summary>
        public decimal Latitude { get; set; }

        /// <summary>Longitud GPS actual. Rango válido: -180 a 180.</summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// Velocidad en km/h (opcional).
        /// Útil para proyecciones de ETA dinámicas en Fase 2.
        /// </summary>
        public decimal? Speed { get; set; }

        /// <summary>
        /// Dirección en grados (0-360, opcional).
        /// Permite rotar el ícono del driver en el mapa del cliente.
        /// </summary>
        public decimal? Heading { get; set; }
    }

    
}
