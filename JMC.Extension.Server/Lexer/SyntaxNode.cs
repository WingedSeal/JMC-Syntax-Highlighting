namespace JMC.Extension.Server.Lexer
{
    internal class SyntaxNode(IEnumerable<SyntaxNode>? previous = null, IEnumerable<SyntaxNode>? next = null)
    {
        public Range Range { get; set; } = new Range();
        public SyntaxNodeType NodeType { get; set; } = SyntaxNodeType.UNKNOWN;
        public IEnumerable<SyntaxNode>? Previous { get; set; } = previous;
        public IEnumerable<SyntaxNode>? Next { get; set; } = next;
    }
}
