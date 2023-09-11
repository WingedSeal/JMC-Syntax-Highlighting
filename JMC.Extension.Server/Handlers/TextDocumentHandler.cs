using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Helper;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace JMC.Extension.Server.Handlers
{
    internal class TextDocumentHandler : TextDocumentSyncHandlerBase
    {
        private readonly ILogger<TextDocumentHandler> _logger;

        private readonly DocumentSelector _documentSelector = new(
            new DocumentFilter
            {
                Pattern = "**/*.jmc"
            }
        );

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Incremental;

        public TextDocumentHandler(ILogger<TextDocumentHandler> logger)
        {
            _logger = logger;
        }
        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Opened Document: ${request.TextDocument.Uri}");
            return Unit.Task;
        }
        public override async Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            foreach (var change in request.ContentChanges)
            {
                var doc = ExtensionData.Workspaces.GetJMCFile(request.TextDocument.Uri);
                if (doc == null)
                    continue;
                var lexer = doc.Lexer;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                lexer.ChangeRawText(change);
                lexer.InitTokens();
                stopwatch.Stop();

                _logger.LogInformation($"Time elapsed for tokenizing: {stopwatch.ElapsedMilliseconds}");
                _logger.LogDebug($"{LoggerHelper.ObjectToJson(change)}");
            }
            _logger.LogDebug($"Changed Document: ${request.TextDocument.Uri}");
            return Unit.Value;
        }
        public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken) {
            _logger.LogDebug($"Saved Document: ${request.TextDocument.Uri}");
            return Unit.Task;
        }
        public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken) {
            _logger.LogDebug($"Closed Document: ${request.TextDocument.Uri}");
            return Unit.Task;
        }

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
            => new(uri, "jmc");
        protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
            SynchronizationCapability capability, ClientCapabilities clientCapabilities)
            => new()
            {
                DocumentSelector = _documentSelector,
                Change = Change,
                Save = new SaveOptions() { IncludeText = true }
            };
    }
}
