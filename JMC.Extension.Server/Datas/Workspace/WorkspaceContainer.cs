using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Lexer.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace JMC.Extension.Server.Datas.Workspace
{
    internal class WorkspaceContainer : List<Workspace>
    {
        public class TokenQueryResult
        {
            public Workspace Workspace { get; set; }
            public DocumentUri DocumentUri { get; set; }
            public List<JMCToken> Tokens { get; set; }
            public TokenQueryResult(Workspace workspace, DocumentUri documentUri, List<JMCToken> tokens)
            {
                Workspace = workspace;
                DocumentUri = documentUri;
                Tokens = tokens;
            }
        }

        public JMCFile? GetJMCFile(DocumentUri uri)
        {
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var f = item.FindJMCFile(uri);
                if (f != null)
                {
                    return f;
                }
            }
            return null;
        }

        public List<TokenQueryResult> GetJMCVariables()
        {
            var tokens = new List<TokenQueryResult>();
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var files = item.JMCFiles.ToArray().AsSpan();
                for (var j = 0; j < files.Length; j++)
                {
                    ref var file = ref files[j];
                    var result = new TokenQueryResult(item, file.DocumentUri, file.Lexer.Variables.ToList());
                    tokens.Add(result);
                }
            }
            return tokens;
        }

        public List<TokenQueryResult> GetJMCFunctionCalls()
        {
            var tokens = new List<TokenQueryResult>();
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var files = item.JMCFiles.ToArray().AsSpan();
                for (var j = 0; j < files.Length; j++)
                {
                    ref var file = ref files[j];
                    var result = new TokenQueryResult(item, file.DocumentUri, file.Lexer.FunctionCalls.ToList());
                    tokens.Add(result);
                }
            }
            return tokens;
        }

        public List<TokenQueryResult> GetJMCFunctionDefines()
        {
            var tokens = new List<TokenQueryResult>();
            var items = ToArray().AsSpan();
            for (var i = 0; i < items.Length; i++)
            {
                ref var item = ref items[i];
                var files = item.JMCFiles.ToArray().AsSpan();
                for (var j = 0; j < files.Length; j++)
                {
                    ref var file = ref files[j];
                    var result = new TokenQueryResult(item, file.DocumentUri, file.Lexer.FunctionDefines.ToList());
                    tokens.Add(result);
                }
            }
            return tokens;
        }
    }
}
