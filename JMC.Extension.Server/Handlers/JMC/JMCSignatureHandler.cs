using JMC.Extension.Server.Datas;
using JMC.Extension.Server.Helper;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Handlers.JMC
{
    internal class JMCSignatureHandler : SignatureHelpHandlerBase
    {
        public override async Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken cancellationToken)
        {
            var doc = ExtensionData.Workspaces.GetJMCFile(request.TextDocument.Uri);
            var workspace = ExtensionData.Workspaces.GetWorkspaceByDocument(request.TextDocument.Uri);
            if (doc == null || workspace == null) return null;

            var lexer = doc.Lexer;
            var token = lexer.GetJMCToken(request.Position);
            if (token == null) return null;

            if (!request.Context.IsRetrigger)
            {
                var preToken = lexer.Tokens[lexer.Tokens.IndexOf(token) - 2];
                if (preToken == null || preToken.TokenType != Lexer.JMC.Types.JMCTokenType.LITERAL) return null;
                var splited = preToken.Value.Split('.').Where(v => !string.IsNullOrEmpty(v));
                if (splited.Count() > 2 || splited.Count() == 1) return null;
                var query = ExtensionData.JMCBuiltInFunctions.GetFunction(splited.ElementAt(0), splited.ElementAt(1));
                if (query == null) return null;
                var signatureHelp = new SignatureHelp()
                {
                    Signatures = new Container<SignatureInformation>(new SignatureInformation()
                    {
                        ActiveParameter = 0,
                        Documentation = query.Summary,
                        Label = query.ToDocumentFormat(),
                        Parameters = new Container<ParameterInformation>(query.Arguments.Select(v => new ParameterInformation()
                        {
                            Label = v.ToDocumentArg(),
                            Documentation = v.Summary
                        }))
                    }),
                    ActiveParameter = 0,
                    ActiveSignature = 0
                };
                return signatureHelp;
            }

            return null;
        }

        protected override SignatureHelpRegistrationOptions CreateRegistrationOptions(SignatureHelpCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            DocumentSelector = LanguageSelector.JMC,
            RetriggerCharacters = new string[]
            {
                " ", ","
            },
            TriggerCharacters = new string[]
            {
                "("
            }
        };
    }
}
