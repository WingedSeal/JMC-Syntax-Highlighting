using OmniSharp.Extensions.LanguageServer.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Types
{
    internal class HJMCFile(DocumentUri documentUri)
    {
        public DocumentUri DocumentUri { get; set; } = documentUri;
    }
}
