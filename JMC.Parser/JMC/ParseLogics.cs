namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private JMCParseResult ParseCondition(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var start = GetIndexStartPos(index);

            //TODO:

            var end = GetIndexEndPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.CONDITION;

            return new(node, index);
        }

        //TODO all not finished
        private JMCParseResult ParseDo(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = query.Next().Expect(JMCSyntaxNodeType.LCP);
            var start = GetIndexStartPos(query.Index);

            //TODO:

            var end = GetIndexStartPos(query.Index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.DO;

            return new(node, index );
        }

        private JMCParseResult ParseWhile(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index );
        }

        private JMCParseResult ParseFor(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index );
        }

        private JMCParseResult ParseSwitch(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index );
        }

        private JMCParseResult ParseIf(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index );
        }
    }
}
