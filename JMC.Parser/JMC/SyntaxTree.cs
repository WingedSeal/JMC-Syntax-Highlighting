using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Error.Base;
using JMC.Parser.JMC.Types;
using JMC.Shared.Datas.Minecraft.Types;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace JMC.Parser.JMC
{
    /// <summary>
    /// Use <see cref="InitializeAsync(string)"/> for constructor
    /// </summary>
    internal partial class SyntaxTree
    {
        public List<SyntaxNode> Nodes { get; set; } = [];
        public SyntaxNode[] FlattenedNodes { get; set; } = [];
        public string RawText { get; set; } = string.Empty;
        public string[] SplitText { get; private set; } = [];
        public string[] TrimmedText { get; private set; } = [];
        public List<JMCBaseError> Errors { get; set; } = [];
        public CancellationTokenSource CancellationSource { get; private set; } = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<SyntaxTree> InitializeAsync(string text)
        {
            Errors.Clear();
            Nodes.Clear();
            RawText = text;
            var lexedText = new Lexer(text).StartLexing();
            SplitText = lexedText.Where(v => v != "").ToArray();
            TrimmedText = SplitText.Select(x => x.Trim()).ToArray();
            await InitAsync();
            FlattenedNodes = [.. GetFlattenNodes()];
            return this;
        }

        //TODO: Not Finished
        public void ModifyIncremental(TextDocumentContentChangeEvent eventArgs)
        {
            if (eventArgs.Range == null)
                return;
            var start = ToOffset(eventArgs.Range.Start);
            var end = ToOffset(eventArgs.Range.End);
            var modifier = eventArgs.Text.Length - (end - start);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Diagnostic[] GetDiagnostics()
        {
            var diagnostics = new List<Diagnostic>();

            var errorsSpan = CollectionsMarshal.AsSpan(Errors);
            for (var i = 0; i < errorsSpan.Length; i++)
            {
                ref var currentError = ref errorsSpan[i];
                diagnostics.Add(new()
                {
                    Range = currentError.Range,
                    Severity = currentError.DiagnosticSeverity,
                    Message = currentError.Message,
                });
            }

            return [.. diagnostics];
        }

        /// <summary>
        /// Reset a tree
        /// </summary>
        /// <param name="changedText"></param>
        public void ModifyFull(string changedText) => InitializeAsync(changedText).Wait();

        /// <inheritdoc cref="ModifyFull(string)"/>
        public async Task ModifyFullAsync(string changedText) => await InitializeAsync(changedText);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync() => await ParseNextAsync(SkipToValue(0), CancellationSource.Token);
        /// <summary>
        /// print a tree view
        /// </summary>
        public void PrintPretty() => Nodes.ForEach(v => v.PrintPretty("", true));
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SyntaxNode> GetFlattenNodes() => Nodes.SelectMany(v => v.ToFlattenNodes());
        /// <summary>
        /// parse next expression
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cancelToken"></param>
        /// <returns>-1 is success,>= 0 is stopped index point</returns>
        /// <exception cref="OperationCanceledException"/>
        public async Task ParseNextAsync(int index, CancellationToken cancelToken)
        {
            if (index == -1)
                return;

            var parseResult = Parse(index, isTopLevelCall: true);
            index = NextIndex(parseResult.EndIndex);
            if (parseResult.Node != null) Nodes.Add(parseResult.Node);

            if (index < TrimmedText.Length - 1)
                await ParseNextAsync(index, cancelToken);
        }
        /// <summary>
        /// Parse a text
        /// </summary>
        /// <param name="index">index of current <seealso cref="TrimmedText"/></param>
        /// <param name="noNextParsed">Does it require to parse the children</param>
        /// <param name="isTopLevelCall">Is it not a call from a parent node</param>
        /// <returns></returns>
        public ParseResult Parse(int index, bool noNextParsed = false, bool isTopLevelCall = false)
        {
            var currentText = TrimmedText[index];
            var nextIndex = NextIndex(index, out _);

            var node = new SyntaxNode();

            var offset = ToOffset(index);
            var range = new Range(GetIndexStartPos(index), GetIndexEndPos(index));
            node.Range = range;
            node.Value = currentText;
            node.Offset = offset;

            switch (currentText)
            {
                #region Keywords
                case "class":
                    return noNextParsed ? new(node, index) : ParseClass(index);
                case "function":
                    return noNextParsed ? new(node, index) : ParseFunction(index);
                case "import":
                    return noNextParsed ? new(node, index) : ParseImport(index);
                case "new":
                    return noNextParsed ? new(node, index) : ParseNew(index);
                case "true":
                    node.NodeType = SyntaxNodeType.True;
                    return new(node, nextIndex);
                case "false":
                    node.NodeType = SyntaxNodeType.False;
                    return new(node, nextIndex);
                case "while":
                    node.NodeType = SyntaxNodeType.While;
                    return new(node, nextIndex);
                case "do":
                    node.NodeType = SyntaxNodeType.Do;
                    return new(node, nextIndex);
                case "for":
                    node.NodeType = SyntaxNodeType.For;
                    return new(node, nextIndex);
                case "break":
                    node.NodeType = SyntaxNodeType.Break;
                    return new(node, nextIndex);
                #endregion

                #region Ops
                case "++":
                    node.NodeType = SyntaxNodeType.IncrementOperator;
                    return new(node, nextIndex);
                case "--":
                    node.NodeType = SyntaxNodeType.DecrementOperator;
                    return new(node, nextIndex);
                case "+":
                    node.NodeType = SyntaxNodeType.PlusOperator;
                    return new(node, nextIndex);
                case "-":
                    node.NodeType = SyntaxNodeType.SubtractOperator;
                    return new(node, nextIndex);
                case "*":
                    node.NodeType = SyntaxNodeType.MultiplyOperator;
                    return new(node, nextIndex);
                case "/":
                    node.NodeType = SyntaxNodeType.DivideOperator;
                    return new(node, nextIndex);
                case "%":
                    node.NodeType = SyntaxNodeType.RemainderOperator;
                    return new(node, nextIndex);
                case "+=":
                    node.NodeType = SyntaxNodeType.PlusEqualOperator;
                    return new(node, nextIndex);
                case "-=":
                    node.NodeType = SyntaxNodeType.SubtractEqualOperator;
                    return new(node, nextIndex);
                case "*=":
                    node.NodeType = SyntaxNodeType.MultiplyEqualOperator;
                    return new(node, nextIndex);
                case "/=":
                    node.NodeType = SyntaxNodeType.DivideEqualOperator;
                    return new(node, nextIndex);
                case "%=":
                    node.NodeType = SyntaxNodeType.RemainderEqualOperator;
                    return new(node, nextIndex);
                case "??=":
                    node.NodeType = SyntaxNodeType.NullcoaleOperator;
                    return new(node, nextIndex);
                case "?=":
                    node.NodeType = SyntaxNodeType.SuccessOperator;
                    return new(node, nextIndex);
                case "><":
                    node.NodeType = SyntaxNodeType.SwapOperator;
                    return new(node, nextIndex);
                #endregion

                #region Comps
                case "||":
                    node.NodeType = SyntaxNodeType.Or;
                    return new(node, nextIndex);
                case "&&":
                    node.NodeType = SyntaxNodeType.And;
                    return new(node, nextIndex);
                case "!":
                    node.NodeType = SyntaxNodeType.Not;
                    return new(node, nextIndex);
                #endregion

                #region Chars
                case "{":
                    node.NodeType = SyntaxNodeType.OpeningCurlyParenthesis;
                    return new(node, nextIndex);
                case "}":
                    node.NodeType = SyntaxNodeType.ClosingCurlyParenthesis;
                    return new(node, nextIndex);
                case "(":
                    node.NodeType = SyntaxNodeType.OpeningParenthesis;
                    return new(node, nextIndex);
                case ")":
                    node.NodeType = SyntaxNodeType.ClosingParenthesis;
                    return new(node, nextIndex);
                case ";":
                    node.NodeType = SyntaxNodeType.Semi;
                    return new(node, nextIndex);
                case ":":
                    node.NodeType = SyntaxNodeType.Colon;
                    return new(node, nextIndex);
                case "=>":
                    node.NodeType = SyntaxNodeType.Arrow;
                    return new(node, nextIndex);
                #endregion

                #region Misc
                case ">":
                    node.NodeType = SyntaxNodeType.GreaterThan;
                    return new(node, nextIndex);
                case "<":
                    node.NodeType = SyntaxNodeType.LessThan;
                    return new(node, nextIndex);
                case ">=":
                    node.NodeType = SyntaxNodeType.GreaterThanEqual;
                    return new(node, nextIndex);
                case "<=":
                    node.NodeType = SyntaxNodeType.LessThanEqual;
                    return new(node, nextIndex);
                case "=":
                    node.NodeType = SyntaxNodeType.EqualTo;
                    return new(node, nextIndex);
                case "==":
                    node.NodeType = SyntaxNodeType.Equal;
                    return new(node, nextIndex);
                case ".":
                    node.NodeType = SyntaxNodeType.Dot;
                    return new(node, nextIndex);
                #endregion
                default:
                    break;
            }

            var specialTokenParseResult = ParseSpecialToken(currentText);

            if (isTopLevelCall)
            {
                var expressionParseResult = ParseExpression(index);
                if (expressionParseResult.Node != null)
                    return expressionParseResult;
            }

            if (specialTokenParseResult != default)
            {
                node.NodeType = specialTokenParseResult;
                return new(node, nextIndex);
            }

            return new(node, nextIndex);
        }
        private static SyntaxNodeType ParseSpecialToken(string value)
        {
            if ((value.StartsWith("//") || value.StartsWith('#')) && !value.Contains(Environment.NewLine))
                return SyntaxNodeType.Comment;
            if (int.TryParse(value, out _))
                return SyntaxNodeType.Int;
            if (float.TryParse(value, out _))
                return SyntaxNodeType.Float;
            if (value == "~")
                return SyntaxNodeType.Tilde;
            if (value == "^")
                return SyntaxNodeType.Caret;

            //selector matching
            var selectorChars = "parse";
            if (value.StartsWith('@') && selectorChars.Contains(value[1]))
                return SyntaxNodeType.Selector;

            //variable matching
            if (value.StartsWith('$'))
            {
                //get value after $
                var valueString = value[1..];
                var isValid = valueString.All(LiteralChars[..(LiteralChars.Length - 1)].Contains);
                if (isValid)
                    return SyntaxNodeType.Variable;
            }
            
            //string matching
            var splitString = value.Split('"');
            if (splitString.Length == 3 && !splitString.Contains(Environment.NewLine))
                return SyntaxNodeType.String;
            splitString = value.Split('\'');
            if (splitString.Length == 3 && !splitString.Contains(Environment.NewLine))
                return SyntaxNodeType.String;
            if (value.StartsWith('`') && value.EndsWith('`'))
                return SyntaxNodeType.MultilineString;

            if (value.All(LiteralChars.Contains))
                return SyntaxNodeType.Literal;

            //literal matching

            return default;
        }
        /// <summary>
        /// parse a import expression
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseImport(int index)
        {
            var node = new SyntaxNode();

            var query = this.AsParseQuery(index);
            var match = query.ExpectList(out _, true, SyntaxNodeType.String, SyntaxNodeType.Semi);

            var startPos = GetIndexStartPos(index);

            //get path
            var pathStringParseResult = Parse(NextIndex(index));

            //move next
            index = SkipToValue(query.Index);

            var endPos = GetIndexStartPos(index);

            //set node
            node.NodeType = SyntaxNodeType.Import;
            node.Next = match && pathStringParseResult.Node != null ? [pathStringParseResult.Node] : null;
            node.Range = new Range(startPos, endPos);

            return new(node, index);
        }
        /// <summary>
        /// parse a class expression
        /// </summary>
        /// <remarks>
        /// literal '{' function* '}'
        /// </remarks>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseClass(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            //check for `literal '{'`
            var query = this.AsParseQuery(index);
            var match = query.ExpectList(out _, true, SyntaxNodeType.Literal, SyntaxNodeType.OpeningCurlyParenthesis);

            //get Key
            var className = TrimmedText[NextIndex(index)];

            //get start pos
            index = SkipToValue(query.Index);
            var startPos = GetIndexStartPos(index);

            //parse functions
            query.Reset(this, index);
            while (index < TrimmedText.Length && match)
            {
                query.Next();
                var isFunction = query.Expect("function", out _, false);
                var isNew = query.Expect("new", out _, false);
                if (isFunction)
                {
                    var functionParseResult = ParseFunction(query.Index);
                    if (functionParseResult.Node != null) next.Add(functionParseResult.Node);
                    index = functionParseResult.EndIndex;
                    query.Reset(this, index);
                }
                else if (isNew)
                {
                    var newParseResult = ParseNew(query.Index);
                    if (newParseResult.Node != null) next.Add(newParseResult.Node);
                    index = newParseResult.EndIndex;
                    query.Reset(this, index);
                }
                else
                {
                    index = NextIndex(index);
                    if (TrimmedText[index] == "}") break;
                    query.Reset(this, index);
                }
            }
            var endPos = GetIndexStartPos(index);

            //set next
            node.NodeType = SyntaxNodeType.Class;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.Value = className;

            return new(node, index);
        }
        /// <summary>
        /// parse a function expression
        /// </summary>
        /// <remarks>
        /// literal '(' ')' block
        /// </remarks>
        /// <param name="index">current index</param>
        /// <returns></returns>
        private ParseResult ParseFunction(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = query.
                ExpectList(out _, true, SyntaxNodeType.Literal, SyntaxNodeType.OpeningParenthesis, SyntaxNodeType.ClosingParenthesis, SyntaxNodeType.OpeningCurlyParenthesis);

            index = SkipToValue(index);

            var functionName = TrimmedText[NextIndex(index)];
            index = query.Index;

            var startPos = GetIndexStartPos(index);

            //parse expressions
            if (match)
            {
                var blockParseResult = ParseBlock(index);
                var blockNode = blockParseResult.Node;
                if (blockNode != null)
                    next.Add(blockNode);
                index = blockParseResult.EndIndex;
            }

            var endPos = GetIndexStartPos(index);
            //set next
            node.NodeType = SyntaxNodeType.Function;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, endPos);
            node.Value = functionName;

            return new(node, index);
        }
        private ParseResult ParseNew(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var query = this.AsParseQuery(index);

            var startPos = GetIndexStartPos(index);
            var match = query.ExpectList(out var list, true, SyntaxNodeType.Literal, SyntaxNodeType.OpeningParenthesis, SyntaxNodeType.Literal, SyntaxNodeType.ClosingParenthesis, SyntaxNodeType.OpeningCurlyParenthesis);
            index = NextIndex(query.Index);
            if (match && list != null)
            {
                //check file type
                var jsonFileType = list.First().Value ?? string.Empty;
                var isValidType = JSONFileTypes.Contains(jsonFileType);
                if (!isValidType)
                    Errors.Add(new JMCSyntaxError(GetRangeByIndex(index), "Unexpected File Type"));

                //read json
                var trimmedTextSpan = TrimmedText.AsSpan(index);
                var tempString = "{";
                var parenthesisCounter = 1;
                for (var i = 0; i < trimmedTextSpan.Length; i++)
                {
                    ref var currentText = ref trimmedTextSpan[i];
                    tempString += currentText;
                    parenthesisCounter += currentText switch
                    {
                        "{" => 1,
                        "}" => -1,
                        _ => 0
                    };
                    if (parenthesisCounter == 0) break;
                }


                //TODO: unfinished
            }
            var end = GetIndexStartPos(index);

            node.NodeType = SyntaxNodeType.New;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(startPos, end);

            return new(node, index);
        }
        /// <summary>
        /// Get the <seealso cref="Range"/> of a <seealso cref="string"/> in <seealso cref="TrimmedText"/> by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Range GetRangeByIndex(int index, bool isCallFromError = false)
        {
            var startPos = GetIndexStartPos(index);
            var endPos = GetIndexEndPos(index, isCallFromError);
            return new Range(startPos, endPos);
        }
        /// <summary>
        /// Get the <seealso cref="Position"/> of a <seealso cref="string"/> in <seealso cref="TrimmedText"/>'s end pos
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Position GetIndexEndPos(int index, bool isCallFromError = false)
        {
            var errorOffset = isCallFromError ? 0 : 1;
            var offset = ToOffset(index);
            var positionOffset = offset + TrimmedText[index].Length - errorOffset;
            return positionOffset.ToPosition(RawText);
        }
        /// <summary>
        /// Skip to a non-space index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="errorCode">0 if success, 1 if out of range</param>
        internal int SkipToValue(int index, out int errorCode)
        {
            errorCode = 0;
            if (index >= TrimmedText.Length - 1)
            {
                errorCode = 1;
                return TrimmedText.Length - 1;
            }

            var trimmedTextSpan = TrimmedText.AsSpan();

            ref var currentText = ref trimmedTextSpan[index];
            var nextIndex = index;

            while (string.IsNullOrEmpty(currentText) || currentText.StartsWith("//"))
            {
                nextIndex++;
                try
                {
                    currentText = ref trimmedTextSpan[nextIndex]!;
                }
                catch (IndexOutOfRangeException)
                {
                    errorCode = 1;
                    return TrimmedText.Length - 1;
                }
            }

            return nextIndex;
        }
        /// <inheritdoc cref="SkipToValue(int, out int)"/>
        internal int SkipToValue(int index) => SkipToValue(index, out _);
        /// <summary>
        /// Get next non-space character index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int NextIndex(int index) => SkipToValue(index + 1);
        /// <summary>
        /// Get next index with error code
        /// </summary>
        /// <param name="index"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        internal int NextIndex(int index, out int errorCode) => SkipToValue(index + 1, out errorCode);
        /// <summary>
        /// index of <seealso cref="TrimmedText"/> to offset
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int ToOffset(int index)
        {
            var offsetResult = 0;

            var splitTextSpan = SplitText.AsSpan(0, index);
            for (var i = 0; i < splitTextSpan.Length; i++)
            {
                ref var v = ref splitTextSpan[i];
                offsetResult += v.Length;
            }

            return offsetResult;
        }
        /// <summary>
        /// <seealso cref="Position"/> to <seealso cref="int"/>
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        internal int ToOffset(Position position)
        {
            if (position.Line == 0) return position.Character;

            var offset = 0;
            var newLineSplit = RawText.Split(Environment.NewLine);
            var newLintSplitSpan = newLineSplit.AsSpan();

            for (var i = 0; i < position.Line; i++)
            {
                ref var currentLine = ref newLintSplitSpan[i];
                offset += currentLine.Length + Environment.NewLine.Length;
            }
            offset += position.Character;

            return offset;
        }
        /// <summary>
        /// Get the start <seealso cref="Position"/> by index of the <seealso cref="TrimmedText"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Position GetIndexStartPos(int index) => ToOffset(index).ToPosition(RawText);
        /// <summary>
        /// return index of <see cref="FlattenedNodes"/>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>-1 if not found</returns>
        public int? GetIndexByRange(Position pos)
        {
            var node = FlattenedNodes.First(v => v.Range != null && v.Range.Contains(pos));
            var index = Array.IndexOf(FlattenedNodes, node);
            return index == -1 ? null : index;
        }
        /// <summary>
        /// offset of text to <seealso cref="TrimmedText"/> Index
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private int? ToIndex(int offset)
        {
            var currentIndex = 0;

            var splitTextSpan = SplitText.AsSpan();
            for (var i = 0; i < splitTextSpan.Length; i++)
            {
                ref var currentText = ref splitTextSpan[i];
                currentIndex += currentText.Length;
                if (currentIndex + currentText.Length > offset + 1) return i;
            }

            return null;
        }
    }
}
