namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var text = TrimmedText[index];
            JMCParseResult? result = text switch
            {
                "do" => await ParseDoAsync(NextIndex(index)),
                "while" => await ParseWhileAsync(NextIndex(index)),
                "for" => await ParseForAsync(NextIndex(index)),
                "switch" => await ParseSwitchAsync(NextIndex(index)),
                "if" => await ParseIfAsync(NextIndex(index)),
                _ => null,
            };
            if (result == null)
            {
                var current = await ParseAsync(index);
                if (current.Node != null)
                {
                    var r = current.Node.NodeType switch
                    {
                        JMCSyntaxNodeType.VARIABLE => await ParseVariableExpressionAsync(NextIndex(index)),
                        _ => null
                    };
                    //set next
                    node.Next = next.Count != 0 ? next : null;

                    return new(r?.Node, index, ToOffset(index).ToPosition(RawText));
                }
                else return new(null, index, ToOffset(index).ToPosition(RawText));
            }
            else return result;
        }
        //TODO
        private async Task<JMCParseResult> ParseVariableExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();



            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseScoreboardObjExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();


            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseAssignmentExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.Next().ExpectOrAsync([.. OperatorsAssignTokens]);

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }
    }
}
