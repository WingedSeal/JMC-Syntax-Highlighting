using JMC.Parser.JMC.Error;
using JMC.Shared;
using JMC.Shared.Datas.BuiltIn;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        /// <summary>
        /// Parse a block
        /// </summary>
        /// <param name="index"></param>
        /// <returns><seealso cref="JMCSyntaxNode"/> has only <seealso cref="JMCSyntaxNode.Next"/></returns>
        private JMCParseResult ParseBlock(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            while (index < TrimmedText.Length)
            {
                var exp = ParseExpression(NextIndex(index));
                if (exp.Node != null) next.Add(exp.Node);
                index = exp.EndIndex;
                if (TrimmedText[index] == "}") break;
            }

            //set next
            node.Next = next.Count == 0 ? null : next;

            return new(node, index);
        }

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseExpression(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var text = TrimmedText[index];
            JMCParseResult? result = text switch
            {
                "do" => ParseDo(NextIndex(index)),
                "while" => ParseWhile(NextIndex(index)),
                "for" => ParseFor(NextIndex(index)),
                "switch" => ParseSwitch(NextIndex(index)),
                "if" => ParseIf(NextIndex(index)),
                _ => null,
            };
            if (result != null)
                return result;
            var current = Parse(index);
            if (current.Node == null)
                return new(null, index);

            if (ExtensionData.CommandTree.RootCommands.Contains(text))
            {
                var cr = ParseCommandExpressionAsync(index);
                if (cr?.Node != null)
                {
                    node.Next = cr.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION_COMMAND;
                    node.Range = cr.Node.Range;
                    index = cr.EndIndex;
                }
                return new(cr?.Node, index);
            }

            else if (current.Node.NodeType == JMCSyntaxNodeType.VARIABLE)
            {
                var r = ParseVariableExpression(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index);
            }
            else if (current.Node.NodeType == JMCSyntaxNodeType.LITERAL &&
                TrimmedText[NextIndex(index)] == ":")
            {
                var r = ParseScoreboardObjExpression(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index);
            }
            else if (current.Node.NodeType == JMCSyntaxNodeType.LITERAL &&
                TrimmedText[NextIndex(index)] == "(")
            {
                var r = ParseFunctionCall(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.EXPRESSION;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index);
            }
            return new(null, index);
        }

        /// <summary>
        /// Parse Variable
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isRecursion"></param>
        /// <returns></returns>
        private JMCParseResult ParseVariableExpression(int index, bool isRecursion = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var value = TrimmedText[index];
            //start position
            var startPos = GetIndexStartPos(index);
            //parse assignment
            var result = ParseAssignmentExpression(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isRecursion)
            {
                var query = this.AsParseQuery(index);
                query.Expect(JMCSyntaxNodeType.SEMI);
            }

            //end position
            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.Value = value;
            node.NodeType = JMCSyntaxNodeType.VARIABLE;

            return new(node, index);
        }

        /// <summary>
        /// Parse a scoreboard object
        /// </summary>
        /// <remarks>
        /// literal ':' selector
        /// </remarks>
        /// <param name="index"></param>
        /// <param name="isRecursion"></param>
        /// <returns></returns>
        private JMCParseResult ParseScoreboardObjExpression(int index, bool isRecursion = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = query.ExpectList(JMCSyntaxNodeType.COLON, JMCSyntaxNodeType.SELECTOR);
            var value = string.Join("", TrimmedText[index..(query.Index + 1)].Where(v => !string.IsNullOrEmpty(v)));
            index = query.Index;
            var startPos = GetIndexStartPos(index);
            //parse assignment
            var result = ParseAssignmentExpression(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isRecursion)
            {
                query.Reset(this, index);
                query.Expect(JMCSyntaxNodeType.SEMI);
            }


            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.NodeType = JMCSyntaxNodeType.SCOREBOARD;
            node.Value = value;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseAssignmentExpression(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = query.ExpectOr([.. OperatorsAssignTokens]);

            if (match.Item1)
            {
                next.Add((Parse(index)).Node!);
                query.Next();
                index = query.Index;
                if (query.ExpectInt())
                {
                    var r = Parse(index);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (query.Expect(JMCSyntaxNodeType.VARIABLE, false))
                {
                    var r = ParseVariableExpression(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (query.Expect(JMCSyntaxNodeType.LITERAL, false))
                {
                    var r = ParseScoreboardObjExpression(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
            }

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(node, index);
        }

        /// <summary>
        /// parse a function call expression
        /// </summary>
        /// <remarks>
        /// '(' parameters* ')'
        /// </remarks>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseFunctionCall(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //parse parameters
            var functionValue = TrimmedText[index];
            index = NextIndex(index);
            var start = GetIndexStartPos(index);
            var param = ParseParameters(NextIndex(index), functionValue);

            //test for RPAREN
            index = param.EndIndex;
            var query = this.AsParseQuery(index);
            query.Expect(JMCSyntaxNodeType.RPAREN);
            index = NextIndex(query.Index);

            //get end pos
            var end = GetIndexStartPos(index);

            node.Next = param.Node?.Next;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.FUNCTION_CALL;
            node.Value = functionValue;

            return new(node, index);
        }

        /// <summary>
        /// parse parameters in a function call
        /// </summary>
        /// <param name="index"></param>
        /// <param name="funcLiteral"></param>
        /// <returns></returns>
        private JMCParseResult ParseParameters(int index, string funcLiteral)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var split = funcLiteral.Split('.');
            var builtinFunc = ExtensionData.JMCBuiltInFunctions.GetFunction(split.First(), split.Last());
            var pos = GetIndexStartPos(index);
            while (TrimmedText[index] != ")")
            {
                var param = builtinFunc != null ? ParseParameter(index, builtinFunc, next) : ParseParameter(index);
                if (param.Node != null)
                    next.Add(param.Node);
                index = NextIndex(param.EndIndex);
                if (TrimmedText[index] == ",")
                    index = NextIndex(index);
            }
            pos = GetIndexStartPos(index);
            if (builtinFunc != null)
            {
                var args = ExtensionData.JMCBuiltInFunctions.GetRequiredArgs(builtinFunc);
                var nonNamedArgsCount = next.Where(v => v.Next?.Count() == 1).Count();
                if (nonNamedArgsCount < args.Count())
                    Errors.Add(new JMCArgumentError(pos, args.ElementAt(nonNamedArgsCount)));
            }

            node.Next = next.Count == 0 ? null : next;

            return new(node, index);
        }

        /// <summary>
        /// parse a parameter
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseParameter(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);
            // literal '=' (number || literal)
            query.Expect(JMCSyntaxNodeType.LITERAL);
            next.Add(new(JMCSyntaxNodeType.LITERAL, TrimmedText[index], range: GetRangeByIndex(index)));
            index = NextIndex(query.Index);

            query.Next().Expect(JMCSyntaxNodeType.EQUAL_TO);
            next.Add(new(JMCSyntaxNodeType.EQUAL_TO, TrimmedText[index], range: GetRangeByIndex(index)));
            index = NextIndex(query.Index);

            var r = query.Next().ExpectOr(JMCSyntaxNodeType.LITERAL, JMCSyntaxNodeType.NUMBER);
            next.Add(new(r.Item2 ?? JMCSyntaxNodeType.UNKNOWN, TrimmedText[index], range: GetRangeByIndex(index)));
            index = query.Index;
            var end = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = JMCSyntaxNodeType.PARAMETER;
            node.Range = new(start, end);

            return new(node, index);
        }

        /// <summary>
        /// parse a parameter for built-in function
        /// </summary>
        /// <param name="index"></param>
        /// <param name="builtInFunction"></param>
        /// <returns></returns>
        private JMCParseResult ParseParameter(int index, JMCBuiltInFunction builtInFunction, List<JMCSyntaxNode> paramNodes)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var query = this.AsParseQuery(index);
            var start = GetIndexStartPos(index);

            var args = builtInFunction.Arguments;
            var hasEqual = query.Next().Expect(JMCSyntaxNodeType.EQUAL_TO, false);
            JMCFunctionArgument? targetArg = hasEqual || paramNodes.Count > 0 && paramNodes.Last().Next?.Count() > 1
                ? args.FirstOrDefault(v => v.Name == TrimmedText[index])
                : args[paramNodes.Count];

            if (targetArg == null)
            {
                Errors.Add(new JMCArgumentError(start, args[paramNodes.Count]));
                index = query.Index;
            }
            else
            {
                if (hasEqual)
                {
                    var result = Parse(index);
                    var resultNode = result.Node!;
                    next.Add(resultNode);
                    index = NextIndex(index);

                    result = Parse(index);
                    resultNode = result.Node!;
                    next.Add(resultNode);
                    index = NextIndex(index);
                }
                query.Reset(index);

                //TODO: not fully supported
                JMCSyntaxNodeType? expectedType = default;
                bool match = false;
                switch (targetArg.ArgumentType)
                {
                    case "String":
                        var tuple = query.ExpectOr(JMCSyntaxNodeType.STRING, JMCSyntaxNodeType.MULTILINE_STRING);
                        expectedType = tuple.Item2;
                        match = tuple.Item1;
                        break;
                    case "FormattedString":
                        match = query.Expect(JMCSyntaxNodeType.STRING);
                        expectedType = match ? JMCSyntaxNodeType.STRING : null;
                        break;
                    case "Integer":
                        match = query.ExpectInt(false);
                        expectedType = match ? JMCSyntaxNodeType.INT : null;
                        break;
                    case "Float":
                        match = query.Expect(JMCSyntaxNodeType.NUMBER);
                        expectedType = match ? JMCSyntaxNodeType.NUMBER : null;
                        break;
                    case "Keyword":
                    case "Objective":
                        match = query.Expect(JMCSyntaxNodeType.LITERAL);
                        expectedType = match ? JMCSyntaxNodeType.LITERAL : null;
                        break;
                    default:
                        break;
                }
                if (match && expectedType != default)
                {
                    index = query.Index;
                    var r = Parse(index);
                    if (r.Node != null)
                        next.Add(r.Node);
                }
            }

            var end = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = JMCSyntaxNodeType.PARAMETER;
            node.Range = new(start, end);

            return new(node, index);
        }
    }
}
