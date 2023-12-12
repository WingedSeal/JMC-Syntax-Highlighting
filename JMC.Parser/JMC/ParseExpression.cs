using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;
using JMC.Shared;
using JMC.Shared.Datas.BuiltIn;
using System.Collections.Immutable;
using System.Diagnostics;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private static readonly ImmutableArray<JMCSyntaxNodeType?> ParseBlockExcluded = [
            JMCSyntaxNodeType.While,
            JMCSyntaxNodeType.For
        ];

        /// <summary>
        /// Parse a block
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseBlock(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var start = GetIndexStartPos(index);

            var errorCode = 0;
            while (index < TrimmedText.Length && errorCode == 0)
            {
                var exp = ParseExpression(NextIndex(index, out var blockError));
                if (exp.Node != null) next.Add(exp.Node);
                index = exp.EndIndex;
                if (TrimmedText[index] == "}" &&
                    !ParseBlockExcluded.Contains(exp.Node?.NodeType))
                {
                    //check if index needs to move
                    var stackTrace = new StackTrace();
                    var frames = stackTrace.GetFrames();
                    var funcCount = frames.Where(v => v.GetMethod().Name == nameof(ParseBlock)).Count() - 1;

                    if (funcCount > 0)
                    {
                        var nextIndex = NextIndex(index, out errorCode);

                        if (errorCode > 0)
                            Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Missing '}'"));

                        var nextString = TrimmedText[nextIndex];
                        if (nextString == "}")
                        {
                            index = nextIndex;
                        }
                    }

                    break;
                }
                else if (blockError > 0)
                {
                    Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Missing '}'"));
                    break;
                }
            }

            var end = GetIndexEndPos(index);

            //set next
            node.Next = next.Count == 0 ? null : next;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.Block;

            return new(node, index);
        }

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseExpression(int index, bool isForLoop = false)
        {
            var node = new JMCSyntaxNode();

            var text = TrimmedText[index];
            JMCParseResult? result = text switch
            {
                "do" => ParseDo(NextIndex(index)),
                "while" => ParseWhile(NextIndex(index)),
                "for" => ParseFor(NextIndex(index)),
                "switch" => ParseSwitch(NextIndex(index)),
                "if" => ParseIf(NextIndex(index)),
                "break" => Parse(index, true),
                _ => null,
            };
            if (result != null)
                return result;
            var current = Parse(index);
            if (current.Node == null)
                return new(null, index);

            if (ExtensionData.CommandTree.RootCommands.Contains(text))
            {
                var cr = ParseCommandExpression(index);
                if (cr?.Node != null)
                {
                    node.Next = cr.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.ExpressionCommand;
                    node.Range = cr.Node.Range;
                    index = cr.EndIndex;
                }
                return new(cr?.Node, index);
            }

            else if (current.Node.NodeType == JMCSyntaxNodeType.Variable)
            {
                var r = ParseVariableExpression(index, isForLoop);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.Expression;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index);
            }
            else if (current.Node.NodeType == JMCSyntaxNodeType.Literal &&
                TrimmedText[NextIndex(index)] == ":")
            {
                var r = ParseScoreboardObjExpression(index, isForLoop);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.Expression;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index);
            }
            else if (current.Node.NodeType == JMCSyntaxNodeType.Literal &&
                TrimmedText[NextIndex(index)] == "(")
            {
                var r = ParseFunctionCall(index);
                if (r?.Node != null)
                {
                    node.Next = r.Node.Next;
                    node.NodeType = JMCSyntaxNodeType.Expression;
                    node.Range = r.Node.Range;
                    index = r.EndIndex;
                }

                return new(r?.Node, index);
            }

            if (current.Node.NodeType != JMCSyntaxNodeType.RCP)
                Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Unexpected Expression"));

            return new(node, index);
        }

        /// <summary>
        /// Parse Variable
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isRecursion"></param>
        /// <returns></returns>
        private JMCParseResult ParseVariableExpression(int index, bool isRecursion = false, bool isForLoop = false, bool isOp = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var value = TrimmedText[index];
            //start position
            var startOffset = ToOffset(index);
            var startPos = GetIndexStartPos(index);
            //parse assignment
            var result = ParseAssignmentExpression(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isOp && !isRecursion)
            {
                var query = this.AsParseQuery(index);
                if (!isForLoop)
                    query.Expect(JMCSyntaxNodeType.Semi, out _);
                else
                    query.Expect(JMCSyntaxNodeType.RParen, out _);
            }

            //end position
            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.Value = value;
            node.NodeType = JMCSyntaxNodeType.Variable;
            node.Offset = startOffset;

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
        private JMCParseResult ParseScoreboardObjExpression(int index, bool isRecursion = false, bool isForLoop = false, bool isOp = false)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = query.ExpectList(out _, true, JMCSyntaxNodeType.Colon, JMCSyntaxNodeType.Selector);
            var value = string.Join("", TrimmedText[index..(query.Index + 1)].Where(v => !string.IsNullOrEmpty(v)));

            var startOffset = ToOffset(index);
            var startPos = GetIndexStartPos(index);
            index = query.Index;

            //parse assignment
            var result = ParseAssignmentExpression(NextIndex(index));
            if (result.Node?.Next != null)
                next.AddRange(result.Node.Next);
            index = result.EndIndex;

            //check for semi
            if (!isOp && !isRecursion)
            {
                query.Reset(this, index);
                if (!isForLoop)
                    query.Expect(JMCSyntaxNodeType.Semi, out _);
                else
                    query.Expect(JMCSyntaxNodeType.RParen, out _);
            }


            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.NodeType = JMCSyntaxNodeType.Scoreboard;
            node.Value = value;
            node.Offset = startOffset;

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
            var isOperatorAssign = query.ExpectOr(out var opAssignNode, [.. OperatorsAssignTokens]);
            var isOperator = query.ExpectOr(out var opNode, [.. OperatorTokens]);

            if (isOperatorAssign)
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
                else if (query.Expect(JMCSyntaxNodeType.Variable, out _, false))
                {
                    var r = ParseVariableExpression(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (query.Expect(JMCSyntaxNodeType.Literal, out _, false))
                {
                    var r = ParseScoreboardObjExpression(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
            }
            else if (isOperator)
            {
                var n = (Parse(index)).Node!;
                next.Add(n);
                query.Next();
                index = query.Index;
                if (query.ExpectInt())
                {
                    var r = ParseAssignmentExpression(NextIndex(index));
                    if (r.Node != null)
                    {
                        r.Node.NodeType = JMCSyntaxNodeType.Number;
                        r.Node.Value = TrimmedText[index];
                        r.Node.Range = GetRangeByIndex(index);
                        next.Add(r.Node);
                    }
                    index = r.EndIndex;
                }
                else if (query.Expect(JMCSyntaxNodeType.Variable, out _, false))
                {
                    var r = ParseVariableExpression(index, true, false, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (query.Expect(JMCSyntaxNodeType.Literal, out _, false))
                {
                    var r = ParseScoreboardObjExpression(index, true, false, true);
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
            query.Expect(JMCSyntaxNodeType.RParen, out _);
            index = NextIndex(query.Index);

            //get end pos
            var end = GetIndexStartPos(index);

            node.Next = param.Node?.Next;
            node.Range = new(start, end);
            node.NodeType = JMCSyntaxNodeType.FunctionCall;
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
                    Errors.Add(new JMCArgumentError(GetRangeByIndex(index), args.ElementAt(nonNamedArgsCount)));
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
            query.Expect(JMCSyntaxNodeType.Literal, out var syntaxNode);
            next.Add(syntaxNode!);

            query.Next().Expect(JMCSyntaxNodeType.EqualTo, out syntaxNode);
            next.Add(syntaxNode!);
            index = NextIndex(query.Index);

            var r = query.Next().ExpectOr(out syntaxNode, JMCSyntaxNodeType.Literal, JMCSyntaxNodeType.Number);
            next.Add(new(syntaxNode?.NodeType ?? JMCSyntaxNodeType.Unknown, TrimmedText[index], range: GetRangeByIndex(index)));
            index = query.Index;
            var end = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = JMCSyntaxNodeType.Parameter;
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
            var hasEqual = query.Next().Expect(JMCSyntaxNodeType.EqualTo, out _, false);
            JMCFunctionArgument? targetArg = hasEqual || paramNodes.Count > 0 && paramNodes.Last().Next?.Count() > 1
                ? args.FirstOrDefault(v => v.Name == TrimmedText[index])
                : args[paramNodes.Count];

            if (targetArg == null)
            {
                Errors.Add(new JMCArgumentError(GetRangeByIndex(index), args[paramNodes.Count]));
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
                        var success = query.ExpectOr(out var syntaxNode, JMCSyntaxNodeType.String, JMCSyntaxNodeType.MultilineString);
                        expectedType = syntaxNode?.NodeType;
                        match = success;
                        break;
                    case "FormattedString":
                        match = query.Expect(JMCSyntaxNodeType.String, out _);
                        expectedType = match ? JMCSyntaxNodeType.String : null;
                        break;
                    case "Integer":
                        match = query.ExpectInt(false);
                        expectedType = match ? JMCSyntaxNodeType.Int : null;
                        break;
                    case "Float":
                        match = query.Expect(JMCSyntaxNodeType.Number, out _);
                        expectedType = match ? JMCSyntaxNodeType.Number : null;
                        break;
                    case "Keyword":
                    case "Objective":
                        match = query.Expect(JMCSyntaxNodeType.Literal, out _);
                        expectedType = match ? JMCSyntaxNodeType.Literal : null;
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
            node.NodeType = JMCSyntaxNodeType.Parameter;
            node.Range = new(start, end);

            return new(node, index);
        }
    }
}
