using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using FileSystemWatcher = OmniSharp.Extensions.LanguageServer.Protocol.Models.FileSystemWatcher;

namespace JMC.Extension.Server.Handler.JMC
{
    internal class JMCDidChangedWatchFilesHandler(ILogger<JMCDidChangedWatchFilesHandler> logger) : DidChangeWatchedFilesHandlerBase
    {
        private ILogger<JMCDidChangedWatchFilesHandler> _logger = logger;
        public override Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
        {
            var changes = request.Changes.ToArray();
            for (var i = 0; i < changes.Length; i++)
            {
                var change = changes[i];
                _logger.LogDebug($"Event Type of {change.Uri}: {change.Type}");
                switch (change.Type)
                {
                    case FileChangeType.Created:
                        ExtensionDatabase.Workspaces.CreateJMCFile(change.Uri);
                        break;
                    case FileChangeType.Deleted:
                        ExtensionDatabase.Workspaces.DeleteJMCFile(change.Uri);
                        break;
                    default:
                        break;
                }
            }

            return Unit.Task;
        }

        protected override DidChangeWatchedFilesRegistrationOptions CreateRegistrationOptions(
            DidChangeWatchedFilesCapability capability, ClientCapabilities clientCapabilities) => new()
            {
                Watchers = new(
            [
                new FileSystemWatcher()
                {
                    GlobPattern = "**/*.jmc",
                    Kind = WatchKind.Create
                },
                new FileSystemWatcher()
                {
                    GlobPattern = "**/*.jmc",
                    Kind = WatchKind.Delete
                }
            ])
            };
    }
}
