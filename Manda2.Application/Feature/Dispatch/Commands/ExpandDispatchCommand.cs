using Manda2.Application.Common;
using Manda2.Application.Contracts;
using Manda2.Application.Mediator;
using Manda2.Domain.Entities;
using Manda2.Contracts.Enum;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Manda2.Contracts.Enum.DispatchEnums;

namespace Manda2.Application.Feature.Dispatch.Commands
{
    // ────────────────────────────────────────────────────────────────────────
    // COMMAND
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Comando para escalar el dispatch a la siguiente ronda.
    /// Enviado por DispatchWorker o RejectDispatchCommandHandler.
    /// </summary>
    public class ExpandDispatchCommand : ICommand<ExpandDispatchResult>
    {
        /// <summary>ID del OrderGroup que necesita un nuevo intento de dispatch.</summary>
        public int OrderGroupId { get; set; }

        /// <summary>ID del DispatchAttempt que expiró o fue rechazado por todos.</summary>
        public int PreviousAttemptId { get; set; }
    }

}
