using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        public class JMCParseResult(JMCSyntaxNode? node, int endIndex, Position position)
        {
            public JMCSyntaxNode? Node { get; set; } = node;
            public int EndIndex { get; set; } = endIndex;
            public Position Position { get; set; } = position;
        }
    }
}
