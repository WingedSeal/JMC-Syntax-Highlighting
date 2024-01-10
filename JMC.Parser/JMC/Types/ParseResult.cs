using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class SyntaxTree
    {
        public class ParseResult(SyntaxNode? node, int endIndex)
        {
            public SyntaxNode? Node { get; set; } = node;
            public int EndIndex { get; set; } = endIndex;
        }
    }
}
