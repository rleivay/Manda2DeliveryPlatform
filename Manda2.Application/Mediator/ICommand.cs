using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    /// <summary>
    /// Marca un objeto como Command que retorna un resultado.
    /// </summary>
    public interface ICommand<TResult> { }

    /// <summary>
    /// Marca un objeto como Command sin resultado (fire and forget).
    /// </summary>
    public interface ICommand : ICommand<Unit> { }
}
