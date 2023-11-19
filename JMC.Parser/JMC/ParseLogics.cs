using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;
using JMC.Shared;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private JMCParseResult ParseCondition(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);

            while (true)
            {
                var isVar = query.Expect(JMCSyntaxNodeType.VARIABLE, out var sn, false);

                string[] commands = [.. ExtensionData.CommandTree.Nodes["execute"].Children!["if"].Children!.Keys];
                var isCommand = commands.Contains(TrimmedText[index]);

                //parse variable
                if (isVar)
                {
                    next.Add(sn!);
                    index = NextIndex(index);
                    var r = ParseVariableCondition(index);
                    if (r.Node?.Next != null)
                        next.AddRange(r.Node.Next);
                    index = NextIndex(r.EndIndex);
                }
                else if (isCommand)
                {
                    //TODO: support for command condition
                }


                query.Reset(index);
                if (!query.ExpectOr(out var syntaxNode, [.. LogicOperatorTokens]) ||
                    syntaxNode?.NodeType == JMCSyntaxNodeType.RPAREN)
                    break;
                else if (syntaxNode?.NodeType == JMCSyntaxNodeType.LPAREN)
                {
                    index = NextIndex(index);
                    var r = ParseCondition(index);
                    index = NextIndex(r.EndIndex);
                    if (r.Node != null)
                        next.Add(r.Node);
                }
                else
                {
                    next.Add(syntaxNode!);
                    query.Next();
                    index = NextIndex(index);
                }

            }

            var end = GetIndexEndPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.CONDITION;

            return new(node, index);
        }

        private JMCParseResult ParseVariableCondition(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);

            if (query.ExpectOr(out var syntaxNode, [.. ConditionalOperatorTokens]))
            {
                next.Add(syntaxNode!);
                JMCSyntaxNodeType[] types = [JMCSyntaxNodeType.NUMBER, JMCSyntaxNodeType.VARIABLE, JMCSyntaxNodeType.SCOREBOARD];
                var match = query.Next().
                    ExpectOr(out syntaxNode, types);

                if (match && syntaxNode != null)
                    next.Add(syntaxNode!);
                else
                    Errors.Add(new JMCSyntaxError(GetIndexStartPos(index), TrimmedText[index], types));

                index = query.Index;
            }
            else if (query.Expect(out syntaxNode, "matches", false))
            {
                next.Add(syntaxNode!);
                var match = query.Next().ExpectIntRange();
                index = query.Index;
                var range = GetRangeByIndex(index);
                next.Add(new(JMCSyntaxNodeType.INT_RANGE, TrimmedText[index], range: range));
            }
            else if (!query.Expect(JMCSyntaxNodeType.RPAREN, out _))
            {
                Errors.Add(new JMCSyntaxError(start, TrimmedText[index],
                    ConditionalOperatorTokens.Select(v => v.ToTokenString())
                    .Concat(["matches"])
                    .ToArray()));
            }

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
            var start = GetIndexStartPos(query.Index);

            var block = ParseBlock(NextIndex(index));
            if (block.Node != null)
                next.Add(block.Node);
            index = block.EndIndex;

            var end = GetIndexStartPos(query.Index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.NodeType = JMCSyntaxNodeType.DO;

            return new(node, index);
        }

        private JMCParseResult ParseWhile(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index);
        }

        private JMCParseResult ParseFor(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index);
        }

        private JMCParseResult ParseSwitch(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index);
        }

        private JMCParseResult ParseIf(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);

            var condition = ParseCondition(NextIndex(index));
            if (condition.Node != null)
                next.Add(condition.Node);
            index = condition.EndIndex;

            var block = ParseBlock(NextIndex(index));
            index = block.EndIndex;
            if (block.Node != null)
                next.Add(block.Node);

            var end = GetIndexEndPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.NodeType = JMCSyntaxNodeType.IF;
            node.Range = new(start, end);

            return new(node, index);
        }
    }
}
