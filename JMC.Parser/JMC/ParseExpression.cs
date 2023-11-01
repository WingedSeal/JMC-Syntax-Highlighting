
using JMC.Shared;

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
                    if (ExtensionData.CommandTree.RootCommands.Contains(text))
                    {
                        var cr = await ParseCommandExpressionAsync(index);
                        if (cr?.Node != null)
                        {
                            node.Next = cr.Node.Next;
                            node.NodeType = JMCSyntaxNodeType.EXPRESSION_COMMAND;
                            node.Range = cr.Node.Range;
                            index = cr.EndIndex;
                        }
                        return new(cr?.Node, index, IndexToPosition(index));
                    }

                    var r = current.Node.NodeType switch
                    {
                        JMCSyntaxNodeType.VARIABLE => await ParseVariableExpressionAsync(index),
                        JMCSyntaxNodeType.LITERAL => await ParseScoreboardObjExpressionAsync(index),
                        _ => null
                    };
                    if (r?.Node != null)
                    {
                        node.Next = r.Node.Next;
                        node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                        node.Range = r.Node.Range;
                        index = r.EndIndex;
                    }

                    return new(r?.Node, index, IndexToPosition(index));
                }
                else return new(null, index, IndexToPosition(index));
            }
            else return result;
        }

        private async Task<JMCParseResult> ParseVariableExpressionAsync(int index, bool isRecursion = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var value = TrimmedText[index];
            //start position
            var startPos = IndexToPosition(index);
            //parse assignment
            var result = await ParseAssignmentExpressionAsync(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isRecursion)
            {
                var query = this.AsParseQuery(index);
                await query.ExpectAsync(JMCSyntaxNodeType.SEMI);
            }

            //end position
            var endPos = IndexToPosition(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.Value = value;
            node.NodeType = JMCSyntaxNodeType.VARIABLE;

            return new(node, index, IndexToPosition(index));
        }

        private async Task<JMCParseResult> ParseScoreboardObjExpressionAsync(int index, bool isRecursion = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(JMCSyntaxNodeType.COLON, JMCSyntaxNodeType.SELECTOR);
            var value = string.Join("", TrimmedText[index..(query.Index + 1)].Where(v => !string.IsNullOrEmpty(v)));
            index = query.Index;
            var startPos = IndexToPosition(index);
            //parse assignment
            var result = await ParseAssignmentExpressionAsync(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isRecursion)
            {
                query.Reset(this, index);
                await query.ExpectAsync(JMCSyntaxNodeType.SEMI);
            }


            var endPos = IndexToPosition(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.NodeType = JMCSyntaxNodeType.SCOREBOARD;
            node.Value = value;

            return new(node, index, IndexToPosition(index));
        }

        private async Task<JMCParseResult> ParseAssignmentExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectOrAsync([.. OperatorsAssignTokens]);

            if (match)
            {
                next.Add((await ParseAsync(index)).Node!);
                query.Next();
                index = query.Index;
                if (query.ExpectInt())
                {
                    var r = await ParseAsync(index);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (await query.ExpectAsync(JMCSyntaxNodeType.VARIABLE, false))
                {
                    var r = await ParseVariableExpressionAsync(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (await query.ExpectAsync(JMCSyntaxNodeType.LITERAL, false))
                {
                    var r = await ParseScoreboardObjExpressionAsync(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
            }

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(node, index, IndexToPosition(index));
        }
    }
}
