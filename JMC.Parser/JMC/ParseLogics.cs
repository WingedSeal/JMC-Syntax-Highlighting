using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;
using JMC.Shared;
using System.Diagnostics;

namespace JMC.Parser.JMC
{
    internal partial class SyntaxTree
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseCondition(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var query = this.AsParseQuery(index);
            var startPos = GetIndexStartPos(index);

            while (index < TrimmedText.Length)
            {
                var isVariable = query.Expect(SyntaxNodeType.Variable, out var sn, false);

                string[] minecraftCommands = [.. ExtensionData.CommandTree.Nodes["execute"].Children!["if"].Children!.Keys];
                var isCommand = minecraftCommands.Contains(TrimmedText[index]);

                //parse variable
                if (isVariable)
                {
                    next.Add(sn!);
                    index = NextIndex(index);
                    var variableCondition = ParseVariableCondition(index);
                    if (variableCondition.Node?.Next != null)
                        next.AddRange(variableCondition.Node.Next);
                    index = NextIndex(variableCondition.EndIndex);
                }
                else if (isCommand)
                {
                    //TODO: support for command condition
                }


                query.Reset(index);
                if (!query.ExpectOr(out var logicOperatorNode, [.. LogicOperatorTokens]) ||
                    logicOperatorNode?.NodeType == SyntaxNodeType.ClosingParenthesis)
                    break;
                else if (logicOperatorNode?.NodeType == SyntaxNodeType.OpeningParenthesis)
                {
                    index = NextIndex(index);
                    var conditionParseResult = ParseCondition(index);
                    index = NextIndex(conditionParseResult.EndIndex);
                    if (conditionParseResult.Node != null)
                        next.Add(conditionParseResult.Node);
                }
                else
                {
                    next.Add(logicOperatorNode!);
                    query.Next();
                    index = NextIndex(index);
                }

            }

            var endPos = GetIndexEndPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.Range = new Range(startPos, endPos);
            node.NodeType = SyntaxNodeType.Condition;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseVariableCondition(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var query = this.AsParseQuery(index);
            var startPos = GetIndexStartPos(index);

            if (query.ExpectOr(out var conditonalOperatorNode, [.. ConditionalOperatorTokens]))
            {
                next.Add(conditonalOperatorNode!);
                SyntaxNodeType[] validTypes = [SyntaxNodeType.Int, SyntaxNodeType.Variable, SyntaxNodeType.Scoreboard];
                var match = query.Next().ExpectOr(out conditonalOperatorNode, validTypes);

                if (match && conditonalOperatorNode != null)
                    next.Add(conditonalOperatorNode!);
                else
                    Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), TrimmedText[index], validTypes));

                index = query.Index;
            }
            else if (query.Expect("matches", out conditonalOperatorNode, false))
            {
                next.Add(conditonalOperatorNode!);
                var match = query.Next().ExpectIntRange();
                index = query.Index;
                var range = GetRangeByIndex(index);
                next.Add(new(SyntaxNodeType.IntRange, TrimmedText[index], range: range));
            }
            else if (!query.Expect(SyntaxNodeType.ClosingParenthesis, out _))
            {
                Errors.Add(new JMCSyntaxError(new(startPos, startPos), TrimmedText[index],
                    ConditionalOperatorTokens.Select(v => v.ToTokenString())
                    .Concat(["matches"])
                    .ToArray()));
            }

            var endPos = GetIndexEndPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.Range = new Range(startPos, endPos);
            node.NodeType = SyntaxNodeType.Condition;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseDo(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var startPos = GetIndexStartPos(index);

            //parse block
            var blockParseResult = ParseBlock(index);
            if (blockParseResult.Node != null)
                next.Add(blockParseResult.Node);
            index = NextIndex(blockParseResult.EndIndex);
            var query = this.AsParseQuery(index);

            //parse while
            query.Expect(SyntaxNodeType.While, out _);
            index = NextIndex(NextIndex(query.Index));

            //parse condition
            var conditionParseResult = ParseCondition(index);
            if (conditionParseResult.Node != null)
                next.Add(conditionParseResult.Node);
            index = NextIndex(conditionParseResult.EndIndex);

            var endPos = GetIndexStartPos(index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.NodeType = SyntaxNodeType.Do;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseWhile(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var startPos = GetIndexStartPos(index);

            //parse condition
            var conditionParseResult = ParseCondition(NextIndex(index));
            if (conditionParseResult.Node != null)
                next.Add(conditionParseResult.Node);
            index = NextIndex(conditionParseResult.EndIndex);

            //parse block
            var blockParseResult = ParseBlock(index);
            if (blockParseResult.Node != null)
                next.Add(blockParseResult.Node);
            index = blockParseResult.EndIndex;

            var endPos = GetIndexStartPos(index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(startPos, endPos);
            node.NodeType = SyntaxNodeType.While;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseFor(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var start = GetIndexStartPos(index);

            //parse statement 1
            var statement1ParseResult = ParseExpression(NextIndex(index));
            if (statement1ParseResult.Node != null)
                next.Add(statement1ParseResult.Node);
            index = statement1ParseResult.EndIndex;

            //parse statement 2
            var statement2ParseResult = ParseCondition(NextIndex(index));
            if (statement2ParseResult.Node != null)
                next.Add(statement2ParseResult.Node);
            index = statement2ParseResult.EndIndex;

            //parse statement 3
            var statement3ParseResult = ParseExpression(NextIndex(index), true);
            if (statement3ParseResult.Node != null)
                next.Add(statement3ParseResult.Node);
            index = statement3ParseResult.EndIndex;

            //parse block
            var blockParseResult = ParseBlock(NextIndex(index));
            if (blockParseResult.Node != null)
                next.Add(blockParseResult.Node);
            index = blockParseResult.EndIndex;

            var endPos = GetIndexEndPos(index);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(start, endPos);
            node.NodeType = SyntaxNodeType.For;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseSwitch(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            //parse paren
            var startPos = GetIndexEndPos(index);

            var query = this.AsParseQuery(index);
            query.ExpectList(out var nodes, true, SyntaxNodeType.Variable, SyntaxNodeType.ClosingParenthesis,SyntaxNodeType.OpeningCurlyParenthesis);
            index = query.Index;

            //add variable
            next.Add(nodes.First());

            //get next text
            index = NextIndex(index);

            var trimmedTextSpan = TrimmedText.AsSpan();
            ref var currentText = ref trimmedTextSpan[index];
            //parse cases
            while (currentText != "}" && index < TrimmedText.Length)
            {
                //parse case
                var caseParseResult = ParseCase(index);
                if (caseParseResult.Node != null)
                    next.Add(caseParseResult.Node);
                index = caseParseResult.EndIndex;
                currentText = ref trimmedTextSpan[index]!;
            }

            var endPos = GetIndexEndPos(index);

            //check closing
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();
            var funcCount = frames.Where(v => v.GetMethod()?.Name == nameof(ParseBlock)).Count();

            if (funcCount > 0)
            {
                index = NextIndex(index, out var errorCode);
                if (TrimmedText[index] != "}" || errorCode > 0)
                    Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Missing '}'"));
            }

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(startPos, endPos);
            node.NodeType = SyntaxNodeType.Switch;
            node.Value = nodes.FirstOrDefault()?.Value;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseCase(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var startPos = GetIndexStartPos(index);
            var query = this.AsParseQuery(index);

            //test for syntax
            var match = query.ExpectList(out var syntaxNodes, true, SyntaxNodeType.Int, SyntaxNodeType.Colon);

            index = NextIndex(query.Index);

            var trimmedTextSpan = TrimmedText.AsSpan();
            ref var currentText = ref trimmedTextSpan[index];
            //parse cases
            while (currentText != "case" && currentText != "}" && index < TrimmedText.Length)
            {
                //parse case
                var expressionParseResult = ParseExpression(index);
                if (expressionParseResult.Node != null)
                    next.Add(expressionParseResult.Node);
                index = NextIndex(expressionParseResult.EndIndex);
                currentText = ref trimmedTextSpan[index]!;
            }

            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new(startPos, endPos);
            node.NodeType = SyntaxNodeType.Case;
            node.Value = syntaxNodes.FirstOrDefault()?.Value;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseIf(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var startPos = GetIndexStartPos(index);

            //parse condition
            var conditionParseResult = ParseCondition(NextIndex(index));
            if (conditionParseResult.Node != null)
                next.Add(conditionParseResult.Node);
            index = conditionParseResult.EndIndex;

            //parse block
            var blockParseResult = ParseBlock(index);
            index = blockParseResult.EndIndex;
            if (blockParseResult.Node != null)
                next.Add(blockParseResult.Node);

            var endPos = GetIndexEndPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.NodeType = SyntaxNodeType.If;
            node.Range = new(startPos, endPos);

            return new(node, index);
        }
    }
}
