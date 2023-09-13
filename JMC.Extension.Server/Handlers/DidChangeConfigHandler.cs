using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Handlers
{
    internal class DidChangeConfigHandler : DidChangeConfigurationHandlerBase
    {
        public override Task<Unit> Handle(DidChangeConfigurationParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }
    }
}
