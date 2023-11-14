using JMC.Parser.Error.Base;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC
{
    //TODO: cancel when encounter error
    /// <summary>
    /// Use <see cref="InitializeAsync(string)"/> for constructor
    /// </summary>
    internal partial class JMCSyntaxTree
    {
        public List<JMCSyntaxNode> Nodes { get; set; } = [];
        public string RawText { get; set; } = string.Empty;
        public string[] SplitText { get; private set; } = [];
        public string[] TrimmedText { get; private set; } = [];
        public List<JMCBaseError> Errors { get; set; } = [];
        public CancellationTokenSource CancellationSource { get; private set; } = new();

        public async Task<JMCSyntaxTree> InitializeAsync(string text)
        {
            RawText = text;
            var split = new JMCLexer(text).StartLexing();
            SplitText = split.Where(v => v != "").ToArray();
            TrimmedText = SplitText.Select(x => x.Trim()).ToArray();
            await InitAsync();
            return this;
        }

        //TODO: after lsp is done
        public void Modify(TextDocumentContentChangeEvent eventArgs)
        {
            if (eventArgs.Range == null)
                return;
            var start = ToOffset(eventArgs.Range.Start);
            var end = ToOffset(eventArgs.Range.End);
            var modifier = eventArgs.Text.Length - (end - start);

        }

        /// <summary>
        /// offset of text to <seealso cref="TrimmedText"/> Index
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private int ToIndex(int offset)
        {
            var current = 0;

            var arr = SplitText.AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var text = ref arr[i];
                current += text.Length;
                if (current + text.Length > offset + 1) return i;
            }

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            await ParseNextAsync(SkipToValue(0), CancellationSource.Token);
        }

        /// <summary>
        /// print a tree view
        /// </summary>
        public void PrintPretty() => Nodes.ForEach(v => v.PrintPretty("", true));
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JMCSyntaxNode> GetFlattenNodes() => Nodes.SelectMany(v => v.ToFlattenNodes());

        /// <summary>
        /// parse next expression
        /// </summary>
        /// <param name="index"></param>
        /// <param name="token"></param>
        /// <returns>-1 is success,>= 0 is stopped index point</returns>
        /// <exception cref="OperationCanceledException"/>
        public async Task ParseNextAsync(int index, CancellationToken token)
        {
            var result = await ParseAsync(index, isStart: true);
            index = NextIndex(result.EndIndex);
            if (result.Node != null) Nodes.Add(result.Node);

            if (index < TrimmedText.Length - 1)
                await ParseNextAsync(index, token);
        }

        /// <summary>
        /// Parse a text
        /// </summary>
        /// <param name="index">index of current <seealso cref="TrimmedText"/></param>
        /// <param name="noNext">Does it require to parse the children</param>
        /// <param name="isStart">Is it not a call from a parent node</param>
        /// <returns></returns>
        public async Task<JMCParseResult> ParseAsync(int index, bool noNext = false, bool isStart = false)
        {
            var value = TrimmedText[index];
            var nextIndex = NextIndex(index);

            var range = new Range(GetIndexStartPos(index), GetIndexEndPos(index));

            switch (value)
            {
                #region Keywords
                case "class":
                    if (!noNext)
                        return await ParseClassAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index);
                    }
                case "function":
                    if (!noNext)
                        return await ParseFunctionAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index);
                    }
                case "import":
                    if (!noNext)
                        return await ParseImportAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index);
                    }
                case "true":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.TRUE,
                        Range = range
                    },
                    nextIndex);
                case "false":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.FALSE,
                        Range = range
                    },
                    nextIndex);
                case "while":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.WHILE,
                        Range = range
                    },
                    nextIndex);
                case "do":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.DO,
                        Range = range
                    },
                    nextIndex);
                case "for":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.FOR,
                        Range = range
                    },
                    nextIndex);
                #endregion

                #region Ops
                case "++":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_INCREMENT,
                        Range = range
                    },
                    nextIndex);
                case "--":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_DECREMENT,
                        Range = range
                    },
                    nextIndex);
                case "+":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_PLUS,
                        Range = range
                    },
                    nextIndex);
                case "-":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SUBSTRACT,
                        Range = range
                    },
                    nextIndex);
                case "*":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_MULTIPLY,
                        Range = range
                    },
                    nextIndex);
                case "/":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_DIVIDE,
                        Range = range
                    },
                    nextIndex);
                case "+=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_PLUSEQ,
                        Range = range
                    },
                    nextIndex);
                case "-=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SUBSTRACTEQ,
                        Range = range
                    },
                    nextIndex);
                case "*=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_MULTIPLYEQ,
                        Range = range
                    },
                    nextIndex);
                case "/=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_DIVIDEEQ
                    },
                    nextIndex);
                case "??=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_NULLCOALE,
                        Range = range
                    },
                    nextIndex);
                case "?=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SUCCESS,
                        Range = range
                    },
                    nextIndex);
                case "><":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SWAP,
                        Range = range
                    },
                    nextIndex);
                #endregion

                #region Comps
                case "||":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COMP_OR
                    },
                    nextIndex);
                case "&&":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COMP_NOT
                    },
                    nextIndex);
                case "!":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COMP_NOT
                    },
                    nextIndex);
                #endregion

                #region Chars
                case "{":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LCP,
                        Range = range
                    },
                    nextIndex);
                case "}":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.RCP,
                        Range = range
                    },
                    nextIndex);
                case "(":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LPAREN,
                        Range = range
                    },
                    nextIndex);
                case ")":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.RPAREN,
                        Range = range
                    },
                    nextIndex);
                case ";":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.SEMI,
                        Range = range
                    },
                    nextIndex);
                case ":":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COLON,
                        Range = range
                    },
                    nextIndex);
                case "=>":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.ARROW_FUNCTION,
                        Range = range
                    },
                    nextIndex);
                #endregion

                #region Misc
                case ">":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.GREATER_THAN,
                        Range = range
                    },
                    nextIndex);
                case "<":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LESS_THAN,
                        Range = range
                    },
                    nextIndex);
                case ">=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.GREATER_THAN_EQ,
                        Range = range
                    },
                    nextIndex);
                case "<=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LESS_THAN_EQ,
                        Range = range
                    },
                    nextIndex);
                case "=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.EQUAL_TO,
                        Range = range
                    },
                    nextIndex);
                case "==":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.EQUAL,
                        Range = range
                    },
                    nextIndex);
                #endregion
                default:
                    break;
            }

            var result = TokenPatterns.FirstOrDefault(v => v.Value.IsMatch(value)).Key;

            if (isStart)
            {
                var r = await ParseExpressionAsync(index);
                if (r.Node != null)
                    return r;
            }

            if (result != default)
                return new(new(result, range: range, value: value), nextIndex);

            return new(null, nextIndex);
        }


        /// <summary>
        /// parse a class expression
        /// </summary>
        /// <remarks>
        /// literal '{' function* '}'
        /// </remarks>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseClassAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //check for `literal '{'`
            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(JMCSyntaxNodeType.LITERAL, JMCSyntaxNodeType.LCP);

            //get Key
            var literal = TrimmedText[NextIndex(index)];

            //get start pos
            index = SkipToValue(query.Index);
            var start = GetIndexStartPos(index);

            //parse functions
            query.Reset(this, index);
            while (index < TrimmedText.Length && match)
            {
                var funcTest = await query.Next().ExpectAsync("function", false);
                if (funcTest)
                {
                    var result = await ParseFunctionAsync(query.Index);
                    if (result.Node != null) next.Add(result.Node);
                    index = result.EndIndex;
                    query.Reset(this, index);
                }
                else
                {
                    index = NextIndex(index);
                    if (TrimmedText[index] == "}") break;
                    query.Reset(this, index);
                }
            }
            var end = GetIndexStartPos(index);

            //set next
            node.NodeType = JMCSyntaxNodeType.CLASS;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.Value = literal;

            return new(node, index);
        }

        /// <summary>
        /// parse a import expression
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseImportAsync(int index)
        {
            var node = new JMCSyntaxNode();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(JMCSyntaxNodeType.STRING, JMCSyntaxNodeType.SEMI);

            var start = GetIndexStartPos(index);

            //get path
            var str = await ParseAsync(NextIndex(index));

            //move next
            index = SkipToValue(query.Index);

            var end = GetIndexStartPos(index);

            //set node
            node.NodeType = JMCSyntaxNodeType.IMPORT;
            node.Next = match && str.Node != null ? [str.Node] : null;
            node.Range = new Range(start, end);

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
        private async Task<JMCParseResult> ParseFunctionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(JMCSyntaxNodeType.LITERAL, JMCSyntaxNodeType.LPAREN, JMCSyntaxNodeType.RPAREN, JMCSyntaxNodeType.LCP);

            index = SkipToValue(index);

            var literal = TrimmedText[NextIndex(index)];
            index = query.Index;

            var start = GetIndexStartPos(index);

            //parse expressions
            if (match)
            {
                var exps = await ParseBlockAsync(index);
                var expsNode = exps.Node;
                if (expsNode != null && expsNode.Next != null)
                    next.AddRange(expsNode.Next);
                index = exps.EndIndex;
            }

            var end = GetIndexStartPos(index);
            //set next
            node.NodeType = JMCSyntaxNodeType.FUNCTION;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.Value = literal;

            return new(node, index);
        }

        /// <summary>
        /// Get the <seealso cref="Range"/> of a <seealso cref="string"/> in <seealso cref="TrimmedText"/> by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Range GetRangeByIndex(int index)
        {
            var start = GetIndexStartPos(index);
            var end = GetIndexEndPos(index);
            return new Range(start, end);
        }

        /// <summary>
        /// Get the <seealso cref="Position"/> of a <seealso cref="string"/> in <seealso cref="TrimmedText"/>'s end pos
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Position GetIndexEndPos(int index) => (ToOffset(index) + TrimmedText[index].Length - 1).ToPosition(RawText);

        /// <summary>
        /// Skip to a non-space index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int SkipToValue(int index)
        {
            if (index >= TrimmedText.Length - 1)
                return TrimmedText.Length - 1;

            var value = TrimmedText[index];
            var nextIndex = index;

            while (string.IsNullOrEmpty(value) || value.StartsWith("//"))
            {
                nextIndex++;
                value = TrimmedText[nextIndex];
            }

            return nextIndex;
        }

        /// <summary>
        /// Get next non-space character index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int NextIndex(int index) => SkipToValue(index + 1);

        /// <summary>
        /// index of <seealso cref="TrimmedText"/> to offset
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int ToOffset(int index)
        {
            var offset = 0;

            var arr = SplitText.AsSpan(0, index);
            for (var i = 0; i < arr.Length; i++)
            {
                ref var v = ref arr[i];
                offset += v.Length;
            }

            return offset;
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
            var split = RawText.Split(Environment.NewLine);
            var arr = split.AsSpan();

            for (var i = 0; i < position.Line; i++)
            {
                ref var line = ref arr[i];
                offset += line.Length + Environment.NewLine.Length;
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
    }
}
