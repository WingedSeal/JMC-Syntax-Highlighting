using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystemWatcher = OmniSharp.Extensions.LanguageServer.Protocol.Models.FileSystemWatcher;

namespace JMC.Extension.Server.Handlers
{
    internal class JMCDidChangedWatchFilesHandler : DidChangeWatchedFilesHandlerBase
    {
        public override async Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
        {
            foreach (var change in request.Changes)
            {
                if (change.Type == FileChangeType.Created)
                {
                    ExtensionData.Workspaces.AddJMCFile(change.Uri);
                }
                else if (change.Type == FileChangeType.Deleted)
                {
                    ExtensionData.Workspaces.RemoveJMCFile(change.Uri);
                }
            }
            return Unit.Value;
        }

        protected override DidChangeWatchedFilesRegistrationOptions CreateRegistrationOptions(DidChangeWatchedFilesCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            Watchers = new(new FileSystemWatcher[]
            {
                new FileSystemWatcher()
                {
                    GlobPattern = "**/*.jmc"
                }
            })
        };
    }
}
