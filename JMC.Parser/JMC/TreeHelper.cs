using JMC.Parser.Error;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using static JMC.Parser.JMC.JMCSyntaxTree;

namespace JMC.Parser.JMC
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
                var newLineCheck = $"{text[i]}{text[i + 1]}";
                if (newLineCheck.StartsWith(Environment.NewLine))
                {
                    line++;
                    col = 1 - Environment.NewLine.Length;
                }
                else col++;

            }

            return new Position(line, col);
        }

        public static async Task<Position> ToPositionAsync(this int offset, string text)
        {
            var line = 0;
            var col = 0;

            for (var i = 0; i < offset; i++)
            {
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

        public static JMCSyntaxError? ExpectToken(this JMCParseResult parseResult, JMCSyntaxNodeType nodeType)
        {
            if (parseResult.Node == null)
                return new(parseResult.Position, nodeType.ToTokenString(), "");
            else if (parseResult.Node.NodeType == nodeType)
                return new(parseResult.Position, nodeType.ToTokenString(), parseResult.Node.NodeType.ToTokenString());
            return null;
        }

        public static JMCParseQuery AsParseQuery(this JMCSyntaxTree syntaxTree, int index = 0) => new(syntaxTree, index);

        public static string ToTokenString(this JMCSyntaxNodeType nodeType)
        {
            return nodeType switch
            {
                JMCSyntaxNodeType.LCP => "{",
                JMCSyntaxNodeType.RCP => "}",
                JMCSyntaxNodeType.LITERAL => "literal",
                _ => nodeType.ToString(),
            };
        }
    }
}
