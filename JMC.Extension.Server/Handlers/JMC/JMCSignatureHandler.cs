using JMC.Extension.Server.Datas;
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
