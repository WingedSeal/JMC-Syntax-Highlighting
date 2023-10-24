namespace JMC.Parser.JMC
{
    internal class JMCSyntaxNode(JMCSyntaxNodeType nodeType = JMCSyntaxNodeType.UNKNOWN, string key = "", IEnumerable<JMCSyntaxNode>? previous = null, IEnumerable<JMCSyntaxNode>? next = null, Range? range = null)
    {
        public Range? Range { get; set; } = range;
        public JMCSyntaxNodeType NodeType { get; set; } = nodeType;
        public IEnumerable<JMCSyntaxNode>? Previous { get; set; } = previous;
        public IEnumerable<JMCSyntaxNode>? Next { get; set; } = next;
        public string? Key { get; set; } = key;

        public void PrintPretty(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine($"{NodeType} {Key}");

            if (Next != null)
                for (int i = 0; i < Next.Count(); i++)
                    Next.ElementAt(i).PrintPretty(indent, i == Next.Count() - 1);
        }

        public IEnumerable<JMCSyntaxNode> ToFlattenNodes()
        {
            var current = new JMCSyntaxNode[] { this };
            if (Next == null) return current;
            else return current.Concat(Next.SelectMany(v => v.ToFlattenNodes()));
        }
    }
}
