using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        public class JMCParseResult(JMCSyntaxNode? node, int endIndex)
        {
            public JMCSyntaxNode? Node { get; set; } = node;
            public int EndIndex { get; set; } = endIndex;
        }
    }
}
