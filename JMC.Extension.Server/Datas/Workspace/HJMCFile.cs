using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Lexer.HJMC;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace JMC.Extension.Server.Datas.Workspace
{
    internal class HJMCFile
    {
        public HJMCLexer Lexer { get; set; }
        public DocumentUri DocumentUri { get; set; }
        public HJMCFile(DocumentUri Uri)
        {
            DocumentUri = Uri;
            var text = File.ReadAllText(DocumentUri.GetFileSystemPath(Uri));
            Lexer = new HJMCLexer(text);
        }
    }
}
