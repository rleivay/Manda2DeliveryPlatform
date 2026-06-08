using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    /// <summary>
    /// Bus central de Manda2. Punto único de entrada para
    /// Commands y Queries.
    /// </summary>
    public interface ICommandBus
    {
        Task<TResult> SendAsync<TCommand, TResult>(
            TCommand command, CancellationToken ct = default)
            where TCommand : ICommand<TResult>;

        Task<TResult> QueryAsync<TQuery, TResult>(
            TQuery query, CancellationToken ct = default)
            where TQuery : IQuery<TResult>;
    }
}
