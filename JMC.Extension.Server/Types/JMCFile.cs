using JMC.Parser.JMC;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Types
{
    internal class JMCFile(SyntaxTree syntaxTree, DocumentUri documentUri)
    {
        public SyntaxTree SyntaxTree { get; set; } = syntaxTree;
        public DocumentUri DocumentUri { get; set; } = documentUri;
    }
}
