using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Lexer.JMC;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Datas.Workspace
{
    internal class JMCFile
    {
        public JMCLexer Lexer { get; set; }
        public DocumentUri DocumentUri { get; set; }
        public JMCFile(DocumentUri Uri)
        {
            DocumentUri = Uri;
            var text = File.ReadAllText(DocumentUri.GetFileSystemPath(Uri));
            Lexer = new(text);
        }
    }
}
