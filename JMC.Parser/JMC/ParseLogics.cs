namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private async Task<JMCParseResult> ParseDoAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.Next().ExpectAsync(JMCSyntaxNodeType.LCP);
            var start = ToOffset(query.Index).ToPosition(RawText);



            var end = ToOffset(query.Index).ToPosition(RawText);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.DO;

            return new(node, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseWhileAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseForAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseSwitchAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseIfAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }
    }
}
