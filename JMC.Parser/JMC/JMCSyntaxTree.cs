using JMC.Parser.Error.Base;
using JMC.Shared;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        public List<JMCSyntaxNode> Nodes { get; set; } = new();

        public string RawText { get; set; } = string.Empty;

        public string[] SplitText { get; private set; } = [];
        public string[] TrimmedText { get; private set; } = [];
        public List<JMCBaseError> Errors { get; set; } = new();

        public JMCSyntaxTree(string text)
        {
            RawText = text;
            SplitText = SPLIT_PATTERN.Split(RawText);
            TrimmedText = SplitText.Select(x => x.Trim()).ToArray();
            Task.Run(InitAsync).Wait();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync() => await ParseNextAsync(SkipToValue(0));
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
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task ParseNextAsync(int index)
        {
            var result = await ParseAsync(index);
            index = NextIndex(result.EndIndex);
            lock (Nodes)
            {
                if (result.Node != null) Nodes.Add(result.Node);
            }
            if (index < TrimmedText.Length - 1)
                await ParseNextAsync(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="noNext"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public async Task<JMCParseResult> ParseAsync(int index, bool noNext = false)
        {
            var value = TrimmedText[index];
            var nextIndex = NextIndex(index);

            var range = new Range(IndexToPosition(index), (ToOffset(index) + value.Length).ToPosition(RawText));

            switch (value)
            {
                #region Keywords
                case "class":
                    if (!noNext)
                        return await ParseClassAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index, IndexToPosition(index));
                    }
                case "function":
                    if (!noNext)
                        return await ParseFunctionAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index, IndexToPosition(index));
                    }
                case "import":
                    if (!noNext)
                        return await ParseImportAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index, IndexToPosition(index));
                    }
                case "true":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.TRUE,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "false":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.FALSE,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "while":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.WHILE,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "do":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.DO,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "for":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.FOR,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                #endregion

                #region Ops
                case "++":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_INCREMENT,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "--":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_DECREMENT,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "+":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_PLUS,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "-":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SUBSTRACT,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "*":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_MULTIPLY,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "/":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_DIVIDE,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "+=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_PLUSEQ,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "-=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SUBSTRACTEQ,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "*=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_MULTIPLYEQ,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "/=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_DIVIDEEQ
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "??=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_NULLCOALE,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "?=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SUCCESS,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "><":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.OP_SWAP,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                #endregion

                #region Comps
                case "||":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COMP_OR
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "&&":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COMP_NOT
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "!":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COMP_NOT
                    },
                    nextIndex, IndexToPosition(nextIndex));
                #endregion

                #region Chars
                case "{":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LCP,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "}":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.RCP,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "(":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LPAREN,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case ")":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.RPAREN,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case ";":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.SEMI,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case ":":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.COLON,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                #endregion

                #region Misc
                case ">":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.GREATER_THAN,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "<":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LESS_THAN,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case ">=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.GREATER_THAN_EQ,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "<=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.LESS_THAN_EQ,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.EQUAL_TO,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                case "==":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = JMCSyntaxNodeType.EQUAL,
                        Range = range
                    },
                    nextIndex, IndexToPosition(nextIndex));
                #endregion
                default:
                    break;
            }

            var result = TokenPatterns.FirstOrDefault(v => v.Value.IsMatch(value)).Key;
            if (result != default)
                return new(new(result, range: range, value: value), nextIndex, IndexToPosition(nextIndex));
            

            return new(null, nextIndex, IndexToPosition(nextIndex));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// literal '{' expression* '}'
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
            var start = IndexToPosition(index);

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
            var end = IndexToPosition(index);

            //set next
            node.NodeType = JMCSyntaxNodeType.CLASS;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.Value = literal;

            return new(node, index, IndexToPosition(index));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseImportAsync(int index)
        {
            var node = new JMCSyntaxNode();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(JMCSyntaxNodeType.STRING, JMCSyntaxNodeType.SEMI);

            var start = IndexToPosition(index);

            //get path
            var str = await ParseAsync(NextIndex(index));

            //move next
            index = SkipToValue(query.Index);

            var end = IndexToPosition(index);

            //set node
            node.NodeType = JMCSyntaxNodeType.IMPORT;
            node.Next = match ? [str.Node] : null;
            node.Range = new Range(start, end);

            return new(node, index, IndexToPosition(index));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// literal '(' ')' '{' expression* '}'
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

            var start = IndexToPosition(index);

            //parse expressions
            while (index < TrimmedText.Length && match)
            {
                var exp = await ParseExpressionAsync(NextIndex(index));
                if (exp.Node != null) next.Add(exp.Node);
                index = exp.EndIndex;
                if (TrimmedText[index] == "}") break;
            }

            var end = IndexToPosition(index);
            //set next
            node.NodeType = JMCSyntaxNodeType.FUNCTION;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.Value = literal;

            return new(node, index, IndexToPosition(index));
        }

        private async Task<JMCParseResult> ParseFunctionCallAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //parse parameters
            var param = await ParseParameterAsync(NextIndex(index));
            index = SkipToValue(param.EndIndex);
            if (param.Node != null) next.Add(param.Node);

            node.Next = next.Count != 0 ? next : null;

            return new(node, index, IndexToPosition(index));
        }

        private async Task<JMCParseResult> ParseParameterAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            node.Next = next.Count != 0 ? next : null;

            return new(node, index, IndexToPosition(index));
        }

        internal int SkipToValue(int index)
        {
            if (index >= TrimmedText.Length - 1) return index;

            var value = TrimmedText[index];
            var nextIndex = index;

            while (string.IsNullOrEmpty(value) || value.StartsWith("//"))
            {
                nextIndex++;
                value = TrimmedText[nextIndex];
            }

            return nextIndex;
        }

        internal int NextIndex(int index) => SkipToValue(index + 1);

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

        internal Position IndexToPosition(int index) => ToOffset(index).ToPosition(RawText);
    }
}
