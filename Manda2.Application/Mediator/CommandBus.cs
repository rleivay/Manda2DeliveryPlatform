using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Application.Mediator
{
    /// <summary>
    /// Implementación del Bus de Manda2.
    /// Resuelve Handlers desde el contenedor DI de .NET.
    /// Sin dependencias externas.
    /// </summary>
    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider _provider;

        public CommandBus(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<TResult> SendAsync<TCommand, TResult>(
            TCommand command, CancellationToken ct = default)
            where TCommand : ICommand<TResult>
        {
            var handler = _provider
                .GetRequiredService<ICommandHandler<TCommand, TResult>>();
            return await handler.HandleAsync(command, ct);
        }

        public async Task SendAsync<TCommand>(
            TCommand command, CancellationToken ct = default)
            where TCommand : ICommand
        {
            var handler = _provider
                .GetRequiredService<ICommandHandler<TCommand>>();
            await handler.HandleAsync(command, ct);
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(
            TQuery query, CancellationToken ct = default)
            where TQuery : IQuery<TResult>
        {
            var handler = _provider
                .GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync(query, ct);
        }
    }
}
