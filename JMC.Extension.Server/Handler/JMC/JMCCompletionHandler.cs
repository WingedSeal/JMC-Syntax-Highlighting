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
                return GetJMCAsync(request, cancellationToken);
            else if (documentUri.Path.EndsWith(".hjmc"))
                return GetHJMCAsync(request, cancellationToken);
            else
                throw new NotImplementedException();
        }

        private Task<CompletionList> GetJMCAsync(CompletionParams request, CancellationToken cancellationToken)
        {
            var list = new List<CompletionItem>();
            var file = ExtensionDatabase.Workspaces.GetJMCFile(request.TextDocument.Uri);

            if (request.Context == null || file == null)
                return Task.FromResult(CompletionList.From(list));

            var workspace = ExtensionDatabase.Workspaces.GetWorkspaceByUri(file.DocumentUri);
            if (workspace == null)
                return Task.FromResult(CompletionList.From(list));

            var triggerChar = request.Context.TriggerCharacter;
            var triggerType = request.Context.TriggerKind;

            //variables
            if (triggerChar != null &&
                triggerChar == "$" &&
                triggerType == CompletionTriggerKind.TriggerCharacter)
            {
                var arr = workspace.GetAllJMCVariableNames().AsSpan();
                for (var i = 0; i < arr.Length; i++)
                {
                    ref var v = ref arr[i];
                    list.Add(new()
                    {
                        Label = v[1..],
                        Kind = CompletionItemKind.Variable
                    });
                }
            }
            //normal case
            else
            {
                var arr = workspace.GetAllJMCFunctionNames().AsSpan();
                for (var i = 0; i < arr.Length; i++)
                {
                    ref var v = ref arr[i];
                    list.Add(new()
                    {
                        Label = v,
                        Kind = CompletionItemKind.Function
                    });
                }
            }

            return Task.FromResult(CompletionList.From(list));
        }

        private Task<CompletionList> GetHJMCAsync(CompletionParams request, CancellationToken cancellationToken)
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
