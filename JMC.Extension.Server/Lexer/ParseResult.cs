using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Lexer
{
    internal partial class SyntaxTree
    {
        public struct ParseResult(SyntaxNode? node, int endIndex, Position position)
        {
            public SyntaxNode? Node { get; set; } = node;
            public int EndIndex { get; set; } = endIndex;
            public Position Position { get; set; } = position;
        }
    }
}
