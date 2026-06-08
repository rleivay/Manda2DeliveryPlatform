// ═══════════════════════════════════════════════════════════════════════════
// ARCHIVO: AvailableDriverDto.cs
//
// PROPÓSITO: Proyección mínima del Driver para el motor de Dispatch.
//            Solo los campos necesarios para tomar la decisión de asignación.
//            NO expone datos sensibles del repartidor.
// ═══════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Dispatch.Dtos
{
    public class AvailableDriverDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Distancia calculada en kilómetros al primer punto de recogida.
        /// Usada para ordenar la lista: el más cercano primero.
        /// </summary>
        public double DistanceKm { get; set; }

        public decimal LastLatitude { get; set; }
        public decimal LastLongitude { get; set; }
        public DateTime LastLocationUpdateAt { get; set; }
    }
}
