using JMC.Extension.Server.Helper;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System.Diagnostics;

namespace JMC.Extension.Server.Handlers.HJMC
{
    internal class HJMCTextDocumentHandler : TextDocumentSyncHandlerBase
    {
        private readonly ILogger<HJMCTextDocumentHandler> _logger;

        private readonly DocumentSelector _documentSelector = new(
            new DocumentFilter
            {
                Pattern = "**/*.hjmc"
            }
        );

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

        public HJMCTextDocumentHandler(ILogger<HJMCTextDocumentHandler> logger)
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
                var doc = ExtensionData.Workspaces.GetHJMCFile(request.TextDocument.Uri);
                if (doc == null)
                    continue;
                var lexer = doc.Lexer;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                lexer.RawText = change.Text;
                lexer.InitTokens();
                stopwatch.Stop();

                _logger.LogInformation($"Time elapsed for tokenizing hjmc: {stopwatch.ElapsedMilliseconds}");
                _logger.LogDebug($"{LoggerHelper.ObjectToJson(change)}");
            }
            _logger.LogDebug($"Changed Document: ${request.TextDocument.Uri}");
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

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
            => new(uri, "hjmc");
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
