using JMC.Extension.Server.Lexer;
using JMC.Extension.Server.Lexer.Error;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using static JMC.Extension.Server.Lexer.SyntaxTree;

namespace JMC.Extension.Server.Helper
{
    internal static class TreeHelper
    {
        public static Position ToPosition(this int offset, string text)
        {
            var line = 0;
            var col = 0;

            var arr = text.ToCharArray().AsSpan();
            for (var i = 0; i < offset; i++)
            {
                ref var value = ref arr[i];
                if (text[i] == '\n')
                {
                    line++;
                    col = 0;
                }
                else
                {
                    col++;
                }
            }

            return new Position(line, col);
        }
        public static SyntaxError? ExpectToken(this ParseResult parseResult, SyntaxNodeType nodeType)
        {
            if (parseResult.Node == null) 
                return new(parseResult.Position, nodeType.ToTokenString(), "");
            else if (parseResult.Node.NodeType == nodeType) 
                return new(parseResult.Position, nodeType.ToTokenString(), parseResult.Node.NodeType.ToTokenString());
            return null;
        }

        public static ParseQuery AsParseQuery(this SyntaxTree syntaxTree, int index = 0) => new(syntaxTree, index);
        

        public static string ToTokenString(this SyntaxNodeType nodeType)
        {
            return nodeType switch
            {
                SyntaxNodeType.LCP => "{",
                SyntaxNodeType.RCP => "}",
                SyntaxNodeType.LITERAL => "literal",
                _ => "",
            };
        }
    }
}
