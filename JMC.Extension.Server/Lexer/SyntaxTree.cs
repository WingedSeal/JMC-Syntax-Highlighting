using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.Error.Base;
using System.Text.RegularExpressions;

namespace JMC.Extension.Server.Lexer
{
    internal partial class SyntaxTree
    {
        public List<SyntaxNode> Nodes { get; set; } = new();

        public string RawText { get; set; } = string.Empty;

        public string[] SplitText { get; private set; } = [];
        public string[] TrimmedText { get; private set; } = [];
        public List<BaseError> Errors { get; set; } = new();

        private static readonly Regex SPLIT_PATTERN = SplitPatternRegex();
        public SyntaxTree(string text)
        {
            RawText = text;
            Task.Run(InitAsync).Wait();
        }

        public async Task InitAsync()
        {
            SplitText = SPLIT_PATTERN.Split(RawText);
            TrimmedText = SplitText.Select(x => x.Trim()).ToArray();
            var index = 0;
            var result = await ParseAsync(index);
            index = result.EndIndex;
            lock (Nodes)
            {
                if (result.Node != null) Nodes.Add(result.Node);
            }
        }

        public async Task<ParseResult> ParseAsync(int index, bool noNext = false)
        {
            var value = TrimmedText[index];
            var nextIndex = index + 1;

            while (string.IsNullOrEmpty(value))
            {
                nextIndex++;
                value = TrimmedText[nextIndex];
            }

            if (value == "class")
            {
                if (!noNext)
                    return await ParseClassAsync(nextIndex);
                else
                {
                    var node = new SyntaxNode();
                }
            }
            else if (value == "{")
            {
                var node = new SyntaxNode
                {
                    NodeType = SyntaxNodeType.LCP
                };
                return new(node, nextIndex, nextIndex.ToPosition(RawText));
            }
            else if (value == "}")
            {
                var node = new SyntaxNode
                {
                    NodeType = SyntaxNodeType.RCP
                };
                return new(node, nextIndex, nextIndex.ToPosition(RawText));
            }

            return new(null, nextIndex, nextIndex.ToPosition(RawText));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// literal '{' expression* '}'
        /// </remarks>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<ParseResult> ParseClassAsync(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            //check for `literal '{'`
            var query = this.AsParseQuery(index);
            var literal = await query.Next().Expect(SyntaxNodeType.LITERAL);
            var start = await query.Next().Expect(SyntaxNodeType.LCP);
            index = query.Index;

            //parse functions
            while (index < TrimmedText.Length && literal && start)
            {
                var expQuery = this.AsParseQuery();
                if (await expQuery.Next().Expect(SyntaxNodeType.FUNCTION))
                {
                    var result = await ParseFunctionAsync(index);
                    if (result.Node != null) next.Add(result.Node);
                    index = result.EndIndex;

                    //break if EOL or meet '}'
                    if (result.Node == null || result.Node.NodeType == SyntaxNodeType.RCP) break;
                }
            }

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(node, index, index.ToPosition(RawText));
        }

        private async Task<ParseResult> ParseFunctionAsync(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, index.ToPosition(RawText));
        }

        private async Task<ParseResult> ParseExpressionAsync(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, index.ToPosition(RawText));
        }

        //private async Task<ParseResult> ExpectNextAsync(int index, SyntaxNodeType nodeType)
        //{
        //    var result = await ParseAsync(index + 1);
        //    TryAddError(result.ExpectToken(nodeType));
        //    return result;
        //}

        [GeneratedRegex(@"(\/\/.*)|(\`(?:.|\s)*\`)|(-?\d*\.?\b\d+[lbs]?\b)|(\.\.\d+)|([""\'].*[""\'])|(\s|\;|\{|\}|\[|\]|\(|\)|\|\||&&|==|!=|[\<\>]\=|[\<\>]|!|,|:|\=\>|[\+\-\*\%\/]\=|[\+\-\*\%\/]|\=)")]
        private static partial Regex SplitPatternRegex();
    }
}
