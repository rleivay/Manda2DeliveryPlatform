using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Contracts.Common
{
    /// <summary>
    /// Wrapper genérico para todas las respuestas de la API.
    /// Permite al MAUI manejar errores y datos de forma uniforme.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
