using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    /// <summary>
    /// Marca un objeto como Query (solo lectura, nunca modifica estado).
    /// </summary>
    public interface IQuery<TResult> { }
}
