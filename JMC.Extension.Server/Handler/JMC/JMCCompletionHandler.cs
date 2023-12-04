using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Immutable;

namespace JMC.Extension.Server.Handler.JMC
{
    internal class JMCCompletionHandler(ILogger<JMCCompletionHandler> logger) : CompletionHandlerBase
    {
        private readonly ILogger<JMCCompletionHandler> _logger = logger;
        public static readonly ImmutableArray<string> TriggerChars = [".", "#", " ", "/", "$"];

        public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken) =>
            Task.FromResult(request);

        public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            var documentUri = request.TextDocument.Uri;

            if (documentUri.Path.EndsWith(".jmc"))
                return GetJMC(request, cancellationToken);
            else if (documentUri.Path.EndsWith(".hjmc"))
                return GetHJMC(request, cancellationToken);
            else
                throw new NotImplementedException();
        }

        private Task<CompletionList> GetJMC(CompletionParams request, CancellationToken cancellationToken)
        {
            var list = new List<CompletionItem>();

            if (request.Context == null)
                return Task.FromResult(CompletionList.From(list));
            var triggerChar = request.Context.TriggerCharacter;
            var triggerType = request.Context.TriggerKind;

            return Task.FromResult(CompletionList.From(list));
        }

        private Task<CompletionList> GetHJMC(CompletionParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new CompletionList());
        }

        protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            ResolveProvider = true,
            TriggerCharacters = TriggerChars,
            DocumentSelector = ExtensionSelector.JMC
        };
    }
}
