using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace JMC.Extension.Server.Handler.JMC
{
    internal class JMCTextDocumentHandler(ILogger<JMCTextDocumentHandler> logger) : TextDocumentSyncHandlerBase
    {
        private ILogger<JMCTextDocumentHandler> _logger = logger;
        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Opened Document: ${request.TextDocument.Uri}");
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
                    await file.SyntaxTree.ModifyFullAsync(request.ContentChanges.First().Text);
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
