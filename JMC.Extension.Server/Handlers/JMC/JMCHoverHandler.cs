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
    internal class JMCHoverHandler : HoverHandlerBase
    {
        public override async Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
        {
            var workspace = ExtensionData.Workspaces.GetWorkspaceByDocument(request.TextDocument.Uri);
            if (workspace == null) return null;
            var file = workspace.FindJMCFile(request.TextDocument.Uri);
            if (file == null) return null;
            var lexer = file.Lexer;
            var token = lexer.GetJMCToken(request.Position);
            if (token == null) return null;
            var splited = token.Value.Split('.');
            var @class = splited[0];
            var func = splited[1];

            var query = ExtensionData.JMCBuiltInFunctions.Find(v => v.Class == @class && v.Function == func);
            if (query == null) return null;
            return new()
            {
                Contents = new(query.ToDocumentFormat())
            };
        }

        protected override HoverRegistrationOptions CreateRegistrationOptions(HoverCapability capability, ClientCapabilities clientCapabilities) => new()
        {
            DocumentSelector = LanguageSelector.JMC
        };
    }
}
