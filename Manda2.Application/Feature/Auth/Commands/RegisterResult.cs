using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Feature.Auth.Commands
{
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        /// <summary>ID del AppUser creado.</summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Indica si la cuenta requiere aprobación manual (Driver, Merchant).
        /// Si true, el usuario no puede hacer login hasta ser aprobado.
        /// </summary>
        public bool RequiresApproval { get; set; }
    }
}
