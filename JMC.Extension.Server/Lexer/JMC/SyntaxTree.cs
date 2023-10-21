using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.Error.Base;
using JMC.Extension.Server.Lexer.JMC;

namespace JMC.Extension.Server.Lexer.JMC
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

        public async Task InitAsync()
        {
            await ParseNextAsync(SkipToValue(0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task ParseNextAsync(int index)
        {
            var result = await ParseAsync(index);
            index = SkipToValue(result.EndIndex + 1);
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
            var nextIndex = SkipToValue(index + 1);

            var range = new Range(ToOffset(index).ToPosition(RawText), (ToOffset(index) + value.Length).ToPosition(RawText));

            switch (value)
            {
                #region Keywords
                case "class":
                    if (!noNext)
                        return await ParseClassAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index, ToOffset(index).ToPosition(RawText));
                    }
                case "function":
                    if (!noNext)
                        return await ParseFunctionAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index, ToOffset(index).ToPosition(RawText));
                    }
                case "import":
                    if (!noNext)
                        return await ParseImportAsync(index);
                    else
                    {
                        return new(new JMCSyntaxNode(), index, ToOffset(index).ToPosition(RawText));
                    }
                case "true":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.TRUE
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "false":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.FALSE
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "while":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.WHILE
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "do":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.DO
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "for":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.FOR
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                #endregion

                #region Ops
                case "++":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_INCREMENT
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "--":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_DECREMENT
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "+":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_PLUS
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "-":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_SUBSTRACT
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "*":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_MULTIPLY
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "/":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_DIVIDE
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "+=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_PLUSEQ
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "-=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_SUBSTRACTEQ
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "*=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_MULTIPLYEQ
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "/=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_DIVIDEEQ
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "??=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_NULLCOALE
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "?=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_SUCCESS
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "><":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.OP_SWAP
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                #endregion

                #region Comps
                case "||":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.COMP_OR
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "&&":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.COMP_NOT
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "!":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.COMP_NOT
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                #endregion

                #region Chars
                case "{":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.LCP
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "}":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.RCP
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "(":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.LPAREN
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case ")":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.RPAREN
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case ";":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.SEMI
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                #endregion

                #region Misc
                case ">":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.GREATER_THAN
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "<":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.LESS_THAN
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case ">=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.GREATER_THAN_EQ
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "<=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.LESS_THAN_EQ
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "=":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.EQUAL_TO
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                case "==":
                    return new(new JMCSyntaxNode
                    {
                        NodeType = SyntaxNodeType.EQUAL
                    },
                    nextIndex, ToOffset(nextIndex).ToPosition(RawText));
                #endregion
                default:
                    break;
            }

            var result = TokenPatterns.FirstOrDefault(v => v.Value.IsMatch(value)).Key;
            if (result != default)
            {
                return new(new(result, range: range), nextIndex, nextIndex.ToPosition(RawText));
            }

            return new(null, nextIndex, ToOffset(nextIndex).ToPosition(RawText));
        }

        /// <summary>
        /// print a tree view
        /// </summary>
        public void PrintPretty() => Nodes.ForEach(v => v.PrintPretty("", true));
        public IEnumerable<JMCSyntaxNode> GetFlattenNodes() => Nodes.SelectMany(v => v.ToFlattenNodes());

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
            var match = await query.ExpectListAsync(ClassPattern);

            //get Key
            var literal = TrimmedText[SkipToValue(index + 1)];

            //get start pos
            index = SkipToValue(query.Index);
            var start = ToOffset(index).ToPosition(RawText);

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
                }
                else
                {
                    index = SkipToValue(index + 1);
                    if (TrimmedText[index] == "}") break;
                }
            }
            var end = ToOffset(index).ToPosition(RawText);

            //set next
            node.NodeType = SyntaxNodeType.CLASS;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.Key = literal;

            return new(node, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseImportAsync(int index)
        {
            var node = new JMCSyntaxNode();

            var query = this.AsParseQuery(index);
            var match = await query.ExpectListAsync(ImportPattern);

            var start = ToOffset(index).ToPosition(RawText);

            //get path
            var str = await ParseAsync(SkipToValue(index + 1));

            //move next
            index = SkipToValue(query.Index);

            var end = ToOffset(index).ToPosition(RawText);

            //set node
            node.NodeType = SyntaxNodeType.IMPORT;
            node.Next = match ? [str.Node] : null;
            node.Range = new Range(start, end);

            return new(node, index, ToOffset(index).ToPosition(RawText));
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
            var match = await query.ExpectListAsync(FunctionPattern);

            index = SkipToValue(index);

            var literal = TrimmedText[SkipToValue(index + 1)];
            index = query.Index;

            var start = ToOffset(index).ToPosition(RawText);

            //parse expressions
            while (index < TrimmedText.Length && match)
            {
                var exp = await ParseExpressionAsync(SkipToValue(index + 1));
                if (exp.Node != null) next.Add(exp.Node);
                index = exp.EndIndex;
                if (TrimmedText[index] == "}") break;
            }

            var end = ToOffset(index).ToPosition(RawText);
            //set next
            node.NodeType = SyntaxNodeType.FUNCTION;
            node.Next = next.Count != 0 ? next : null;
            node.Range = new Range(start, end);
            node.Key = literal;

            return new(node, index, ToOffset(index).ToPosition(RawText));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// //TODO
        private async Task<JMCParseResult> ParseExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            var text = TrimmedText[index];
            JMCParseResult? result = text switch
            {
                "do" => await ParseDoAsync(SkipToValue(index + 1)),
                "while" => await ParseWhileAsync(SkipToValue(index + 1)),
                "for" => await ParseForAsync(SkipToValue(index + 1)),
                "switch" => await ParseSwitchAsync(SkipToValue(index + 1)),
                "if" => await ParseIfAsync(SkipToValue(index + 1)),
                _ => null,
            };
            //TODO
            if (result == null)
            {
                if (ExtensionData.CommandTree.RootCommands.Contains(text))
                {

                }
                else
                {
                    var current = await ParseAsync(index, true);
                }
                //set next
                node.Next = next.Count != 0 ? next : null;

                return new(node, index, ToOffset(index).ToPosition(RawText));
            }
            else
            {
                next.Add(node);
                index = result.EndIndex;
                //set next
                node.Next = next.Count != 0 ? next : null;

                return new(node, index, ToOffset(index).ToPosition(RawText));
            }
        }

        private async Task<JMCParseResult> ParseFunctionCallAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //parse parameters
            var param = await ParseParameterAsync(SkipToValue(index + 1));
            index = SkipToValue(param.EndIndex);
            if (param.Node != null) next.Add(param.Node);

            node.Next = next.Count != 0 ? next : null;

            return new(node, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseParameterAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            node.Next = next.Count != 0 ? next : null;

            return new(node, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseCommandAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            node.Next = next.Count != 0 ? next : null;

            return new(node, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseAssignmentAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            node.Next = next.Count != 0 ? next : null;

            return new(node, index, ToOffset(index).ToPosition(RawText));
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

        private int ToOffset(int index)
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

    }
}
