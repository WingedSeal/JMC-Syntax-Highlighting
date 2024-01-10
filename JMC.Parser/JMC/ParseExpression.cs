using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;
using JMC.Shared;
using JMC.Shared.Datas.BuiltIn;
using System.Collections.Immutable;
using System.Diagnostics;

namespace JMC.Parser.JMC
{
    internal partial class SyntaxTree
    {
        private static readonly ImmutableArray<SyntaxNodeType?> ParseBlockSpecialCases = [
            SyntaxNodeType.While,
            SyntaxNodeType.For
        ];

        /// <summary>
        /// Parse a block
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal ParseResult ParseBlock(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var start = GetIndexStartPos(index);

            var errorCode = 0;
            while (index < TrimmedText.Length && errorCode == 0)
            {
                var exp = ParseExpression(NextIndex(index, out var blockError));
                if (exp.Node != null) next.Add(exp.Node);
                index = exp.EndIndex;
                if (TrimmedText[index] == "}" &&
                    !ParseBlockSpecialCases.Contains(exp.Node?.NodeType))
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
                        else if (nextString == "function" || nextString == "class")
                        {
                            Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Missing '}'"));
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
            node.NodeType = SyntaxNodeType.Block;

            return new(node, index);
        }

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseExpression(int index, bool isForLoop = false)
        {
            var node = new SyntaxNode();

            var text = TrimmedText[index];
            ParseResult? result = text switch
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
            var currentExpression = Parse(index);
            if (currentExpression.Node == null)
                return new(null, index);

            if (ExtensionData.CommandTree.RootCommands.Contains(text))
            {
                var commandExpressionResult = ParseCommandExpression(index);
                if (commandExpressionResult?.Node != null)
                {
                    node.Next = commandExpressionResult.Node.Next;
                    node.NodeType = SyntaxNodeType.ExpressionCommand;
                    node.Range = commandExpressionResult.Node.Range;
                    index = commandExpressionResult.EndIndex;
                }
                return new(commandExpressionResult?.Node, index);
            }

            else if (currentExpression.Node.NodeType == SyntaxNodeType.Variable)
            {
                var variableParseResult = ParseVariableExpression(index, isForLoop);
                if (variableParseResult?.Node != null)
                {
                    node.Next = variableParseResult.Node.Next;
                    node.NodeType = SyntaxNodeType.Expression;
                    node.Range = variableParseResult.Node.Range;
                    index = variableParseResult.EndIndex;
                }

                return new(variableParseResult?.Node, index);
            }
            else if (currentExpression.Node.NodeType == SyntaxNodeType.Literal &&
                TrimmedText[NextIndex(index)] == ":")
            {
                var scoreboardObjParseResult = ParseScoreboardObjExpression(index, isForLoop);
                if (scoreboardObjParseResult?.Node != null)
                {
                    node.Next = scoreboardObjParseResult.Node.Next;
                    node.NodeType = SyntaxNodeType.Expression;
                    node.Range = scoreboardObjParseResult.Node.Range;
                    index = scoreboardObjParseResult.EndIndex;
                }

                return new(scoreboardObjParseResult?.Node, index);
            }
            else if (currentExpression.Node.NodeType == SyntaxNodeType.Literal &&
                TrimmedText[NextIndex(index)] == "(")
            {
                var functionCallParseResult = ParseFunctionCall(index);
                if (functionCallParseResult?.Node != null)
                {
                    node.Next = functionCallParseResult.Node.Next;
                    node.NodeType = SyntaxNodeType.Expression;
                    node.Range = functionCallParseResult.Node.Range;
                    index = functionCallParseResult.EndIndex;
                }

                return new(functionCallParseResult?.Node, index);
            }

            if (currentExpression.Node.NodeType != SyntaxNodeType.ClosingCurlyParenthesis)
                Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Unexpected Expression"));

            return new(null, index);
        }

        /// <summary>
        /// Parse Variable
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isRecursion"></param>
        /// <returns></returns>
        private ParseResult ParseVariableExpression(int index, bool isRecursion = false, bool isForLoop = false, bool isOp = false)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var variableName = TrimmedText[index];

            //start position
            var startOffset = ToOffset(index);
            var startPos = GetIndexStartPos(index);
            if (TrimmedText[NextIndex(index)] != ".")
            {
                //parse assignment
                var assignParseResult = ParseAssignmentExpression(NextIndex(index));
                if (assignParseResult.Node?.Next != null)
                    next.AddRange(assignParseResult.Node.Next);
                index = assignParseResult.EndIndex;

                //check for semi
                if (!isOp && !isRecursion)
                {
                    var query = this.AsParseQuery(index);
                    if (!isForLoop)
                        query.Expect(SyntaxNodeType.Semi, out _);
                    else
                        query.Expect(SyntaxNodeType.ClosingParenthesis, out _);
                }
                node.NodeType = SyntaxNodeType.Variable;
                node.Value = variableName;
            }
            else
            {
                var query = this.AsParseQuery(index);
                var match = query.ExpectList(out var list, true, SyntaxNodeType.Dot, SyntaxNodeType.Literal);
                index = NextIndex(query.Index);
                node.Value = variableName;
                if (match && list != null)
                {
                    var funcName = list[1]?.Value ?? string.Empty;
                    var @params = ParseParameters(NextIndex(index), funcName);
                    if (@params.Node?.Next != null)
                        next.AddRange(@params.Node.Next);
                    index = @params.EndIndex;

                    query.Reset(NextIndex(index));
                    query.Expect(SyntaxNodeType.Semi);
                    index = query.Index;

                    node.Value = $"{variableName}.{funcName}";
                }
                node.NodeType = SyntaxNodeType.VariableCall;
            }
            //end position
            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
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
        private ParseResult ParseScoreboardObjExpression(int index, bool isRecursion = false, bool isForLoop = false, bool isOperator = false)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = query.ExpectList(out _, true, SyntaxNodeType.Colon, SyntaxNodeType.Selector);
            //value is obj:@s
            var scoreboardObjValue = string.Join("", TrimmedText[index..(query.Index + 1)].Where(v => !string.IsNullOrEmpty(v)));

            var startOffset = ToOffset(index);
            var startPos = GetIndexStartPos(index);
            index = query.Index;

            //parse assignment
            var assignParseResult = ParseAssignmentExpression(NextIndex(index));
            if (assignParseResult.Node?.Next != null)
                next.AddRange(assignParseResult.Node.Next);
            index = assignParseResult.EndIndex;

            //check for semi
            if (!isOperator && !isRecursion)
            {
                query.Reset(this, index);
                if (!isForLoop)
                    query.Expect(SyntaxNodeType.Semi, out _);
                else
                    query.Expect(SyntaxNodeType.ClosingParenthesis, out _);
            }


            var endPos = GetIndexStartPos(index);

            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.NodeType = SyntaxNodeType.Scoreboard;
            node.Value = scoreboardObjValue;
            node.Offset = startOffset;

            return new(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseAssignmentExpression(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var query = this.AsParseQuery(index);
            var isOperatorAssign = query.ExpectOr(out var opAssignNode, [.. OperatorsAssignTokens]);
            var isOperator = query.ExpectOr(out var opNode, [.. OperatorTokens]);

            if (isOperatorAssign)
            {
                next.Add(Parse(index).Node!);
                query.Next();
                index = query.Index;
                if (query.ExpectInt())
                {
                    var r = Parse(index);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (query.Expect(SyntaxNodeType.Variable, out _, false))
                {
                    var r = ParseVariableExpression(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
                else if (query.Expect(SyntaxNodeType.Literal, out _, false))
                {
                    var r = ParseScoreboardObjExpression(index, true);
                    next.Add(r.Node!);
                    index = r.EndIndex;
                }
            }
            else if (isOperator)
            {
                var startNode = Parse(index).Node!;
                next.Add(startNode);
                query.Next();
                index = query.Index;
                if (query.ExpectInt())
                {
                    var assignParseResult = ParseAssignmentExpression(NextIndex(index));
                    if (assignParseResult.Node != null)
                    {
                        assignParseResult.Node.NodeType = SyntaxNodeType.Number;
                        assignParseResult.Node.Value = TrimmedText[index];
                        assignParseResult.Node.Range = GetRangeByIndex(index);
                        next.Add(assignParseResult.Node);
                    }
                    index = assignParseResult.EndIndex;
                }
                else if (query.Expect(SyntaxNodeType.Variable, out _, false))
                {
                    var variableParseResult = ParseVariableExpression(index, true, false, true);
                    next.Add(variableParseResult.Node!);
                    index = variableParseResult.EndIndex;
                }
                else if (query.Expect(SyntaxNodeType.Literal, out _, false))
                {
                    var scoreboardObjParseResult = ParseScoreboardObjExpression(index, true, false, true);
                    next.Add(scoreboardObjParseResult.Node!);
                    index = scoreboardObjParseResult.EndIndex;
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
        private ParseResult ParseFunctionCall(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            //parse parameters
            var functionName = TrimmedText[index];
            index = NextIndex(index);
            var startPos = GetIndexStartPos(index);
            var paramsParseResult = ParseParameters(NextIndex(index), functionName);

            //test for RPAREN
            index = paramsParseResult.EndIndex;
            var query = this.AsParseQuery(index);
            query.Expect(SyntaxNodeType.ClosingParenthesis);
            query.Next().Expect(SyntaxNodeType.Semi);
            index = query.Index;

            //get end pos
            var endPos = GetIndexStartPos(index);

            node.Next = paramsParseResult.Node?.Next;
            node.Range = new(startPos, endPos);
            node.NodeType = SyntaxNodeType.FunctionCall;
            node.Value = functionName;

            return new(node, index);
        }

        /// <summary>
        /// parse parameters in a function call
        /// </summary>
        /// <param name="index"></param>
        /// <param name="funcLiteral"></param>
        /// <returns></returns>
        private ParseResult ParseParameters(int index, string funcLiteral)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var splitLiteral = funcLiteral.Split('.');
            var builtinFuncs = ExtensionData.JMCBuiltInFunctions.GetFunction(splitLiteral.First(), splitLiteral.Last());
            var currentPos = GetIndexStartPos(index);
            while (TrimmedText[index] != ")" && index < TrimmedText.Length)
            {
                var paramsParseResult = builtinFuncs != null ? ParseParameter(index, builtinFuncs, next) : ParseParameter(index);
                if (paramsParseResult.Node != null)
                    next.Add(paramsParseResult.Node);
                index = NextIndex(paramsParseResult.EndIndex);
                if (TrimmedText[index] == ",")
                    index = NextIndex(index);
            }

            currentPos = GetIndexStartPos(index);
            if (builtinFuncs != null)
            {
                var requiredArgs = ExtensionData.JMCBuiltInFunctions.GetRequiredArgs(builtinFuncs);
                var nonNamedArgsCount = next.Where(v => v.Next?.Count() == 1).Count();
                if (nonNamedArgsCount < requiredArgs.Count())
                    Errors.Add(new JMCArgumentError(GetRangeByIndex(index), requiredArgs.ElementAt(nonNamedArgsCount)));
            }

            node.Next = next.Count == 0 ? null : next;

            return new(node, index);
        }

        /// <summary>
        /// parse a parameter
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseParameter(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var query = this.AsParseQuery(index);
            var startPos = GetIndexStartPos(index);
            // literal '=' (number || literal)
            query.Expect(SyntaxNodeType.Literal, out var syntaxNode);
            next.Add(syntaxNode!);

            query.Next().Expect(SyntaxNodeType.EqualTo, out syntaxNode);
            next.Add(syntaxNode!);
            index = NextIndex(query.Index);

            query.Next().ExpectOr(out syntaxNode, SyntaxNodeType.Literal, SyntaxNodeType.Number);
            next.Add(new(syntaxNode?.NodeType ?? SyntaxNodeType.Unknown, TrimmedText[index], range: GetRangeByIndex(index)));
            index = query.Index;
            var endPos = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = SyntaxNodeType.Parameter;
            node.Range = new(startPos, endPos);

            return new(node, index);
        }

        /// <summary>
        /// parse a parameter for built-in function
        /// </summary>
        /// <param name="index"></param>
        /// <param name="builtInFunction"></param>
        /// <returns></returns>
        private ParseResult ParseParameter(int index, JMCBuiltInFunction builtInFunction, List<SyntaxNode> paramNodes)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var query = this.AsParseQuery(index);
            var startPos = GetIndexStartPos(index);

            var args = builtInFunction.Arguments;
            var hasEqualSign = query.Next().Expect(SyntaxNodeType.EqualTo, out _, false);
            JMCFunctionArgument? targetArg = hasEqualSign || paramNodes.Count > 0 && paramNodes.Last().Next?.Count() > 1
                ? args.FirstOrDefault(v => v.Name == TrimmedText[index])
                : args[paramNodes.Count];

            if (targetArg == null)
            {
                Errors.Add(new JMCArgumentError(GetRangeByIndex(index), args[paramNodes.Count]));
                index = query.Index;
            }
            else
            {
                if (hasEqualSign)
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
                SyntaxNodeType? expectedType = default;
                bool match = false;
                var argTypeString = targetArg.ArgumentType;
                var jsonString = string.Empty;
                SyntaxNode? syntaxNode = null;
                switch (argTypeString)
                {
                    case "String":
                        var success = query.ExpectOr(out syntaxNode, SyntaxNodeType.String, SyntaxNodeType.MultilineString);
                        expectedType = syntaxNode?.NodeType;
                        match = success;
                        break;
                    case "FormattedString":
                        match = query.Expect(SyntaxNodeType.String, out _);
                        expectedType = match ? SyntaxNodeType.String : null;
                        break;
                    case "Integer":
                        match = query.ExpectInt(false);
                        expectedType = match ? SyntaxNodeType.Int : null;
                        break;
                    case "Float":
                        match = query.Expect(SyntaxNodeType.Number, out _);
                        expectedType = match ? SyntaxNodeType.Number : null;
                        break;
                    case "Boolean":
                        match = query.ExpectOr(out syntaxNode, SyntaxNodeType.True, SyntaxNodeType.False);
                        expectedType = match ? syntaxNode?.NodeType : null;
                        break;
                    case "JSObject":
                    case "JSON":
                        match = query.ExpectJSON(out jsonString);
                        expectedType = match ? SyntaxNodeType.Json : null;
                        break;
                    case "FunctionName":
                    case "Keyword":
                    case "Objective":
                        match = query.Expect(SyntaxNodeType.Literal, out _);
                        expectedType = match ? SyntaxNodeType.Literal : null;
                        break;
                    case "ArrowFunction":
                        match = query.ExpectArrowFunction(out syntaxNode);
                        expectedType = match ? SyntaxNodeType.ArrowFunction : null;
                        break;
                    case "Function":
                        if (TrimmedText[index].StartsWith('('))
                        {
                            match = query.ExpectArrowFunction(out syntaxNode);
                            expectedType = match ? SyntaxNodeType.ArrowFunction : null;
                        }
                        else
                        {
                            match = query.Expect(SyntaxNodeType.Literal, out _);
                            expectedType = match ? SyntaxNodeType.Literal : null;
                        }
                        break;
                    case "TargetSelector":
                    case "Scoreboard":
                    case "ScoreboardInteger":
                    case "Criteria":
                    case "Item":
                        break;
                    default:
                        if (argTypeString.StartsWith("List"))
                        {
                            //TODO Not implemented
                            match = query.Expect(SyntaxNodeType.List, out _);
                            expectedType = match ? SyntaxNodeType.List : null;
                        }
                        break;
                }
                SyntaxNodeType[] specialTypes = [SyntaxNodeType.Json, SyntaxNodeType.List];
                //add node
                if (match && expectedType != default && specialTypes.Contains((SyntaxNodeType)expectedType!))
                {
                    var isArrowFunc = expectedType == SyntaxNodeType.ArrowFunction;
                    var r = new SyntaxNode
                    {
                        NodeType = (SyntaxNodeType)expectedType,
                        Range = new(GetIndexStartPos(index), GetIndexEndPos(query.Index)),
                        Value = jsonString,
                        Next = isArrowFunc ? syntaxNode?.Next : null
                    };
                    next.Add(r);
                }
                else if (match && expectedType != default)
                {
                    index = query.Index;
                    var r = Parse(index);
                    if (r.Node != null)
                        next.Add(r.Node);
                }
            }

            var end = GetIndexStartPos(index);

            node.Next = next.Count == 0 ? null : next;
            node.NodeType = SyntaxNodeType.Parameter;
            node.Range = new(startPos, end);

            return new(node, index);
        }
    }
}
