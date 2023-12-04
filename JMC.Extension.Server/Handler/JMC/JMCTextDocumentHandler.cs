using JMC.Parser.JMC;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace JMC.Extension.Server.Handler.JMC
{
    internal class JMCTextDocumentHandler(ILanguageServerFacade facade, ILogger<JMCTextDocumentHandler> logger) : TextDocumentSyncHandlerBase
    {
        private readonly ILanguageServerFacade _facade = facade;
        private readonly ILogger<JMCTextDocumentHandler> _logger = logger;
        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

        private void PublicDiagnostic(JMCSyntaxTree tree, DocumentUri uri, int? version) => _facade.TextDocument.PublishDiagnostics(new()
        {
            Diagnostics = new(tree.GetDiagnostics()),
            Uri = uri,
            Version = version
        });

        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Opened Document: ${request.TextDocument.Uri}");
            var filePath = request.TextDocument.Uri;
            var file = ExtensionDatabase.Workspaces.GetJMCFile(filePath);
            if (file != null)
                PublicDiagnostic(file.SyntaxTree, file.DocumentUri, request.TextDocument.Version);
            return Unit.Task;
        }

        public override async Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Changed Document: ${request.TextDocument.Uri}");
            _logger.LogDebug($"Change Mode: {Change}");

            var filePath = request.TextDocument.Uri;
            if (Change == TextDocumentSyncKind.Full)
            {
                var file = ExtensionDatabase.Workspaces.GetJMCFile(filePath);
                if (file != null)
                {
                    await file.SyntaxTree.ModifyFullAsync(request.ContentChanges.First().Text);
                    PublicDiagnostic(file.SyntaxTree, file.DocumentUri, request.TextDocument.Version);
                }
            }
            //TODO Incremental support

            return Unit.Value;
        }

        public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Saved Document: ${request.TextDocument.Uri}");
            return Unit.Task;
        }
        public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Closed Document: ${request.TextDocument.Uri}");
            return Unit.Task;
        }

        protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(SynchronizationCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            DocumentSelector = ExtensionSelector.JMC,
            Change = Change,
            Save = new SaveOptions()
            {
                IncludeText = true
            }
        };

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) => new(uri, "jmc");
    }
}
