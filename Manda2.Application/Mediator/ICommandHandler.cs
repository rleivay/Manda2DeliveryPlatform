using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    /// <summary>Handler para Commands con resultado.</summary>
    public interface ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
    }

    /// <summary>Handler para Commands sin resultado (retorna Unit).</summary>
    public interface ICommandHandler<TCommand>
        : ICommandHandler<TCommand, Unit>
        where TCommand : ICommand<Unit>
    {
    }
}
