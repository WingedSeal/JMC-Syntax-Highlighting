using JMC.Parser.JMC.Error;

namespace JMC.Parser.JMC
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="syntaxTree"></param>
    /// <param name="startIndex"></param>
    internal class JMCParseQuery(JMCSyntaxTree syntaxTree, int startIndex = 0)
    {
        private JMCSyntaxTree SyntaxTree { get; set; } = syntaxTree;
        public int Index { get; set; } = startIndex;
        public int PreviousIndex { get; set; } = -1;
        public JMCParseQuery Next()
        {
            PreviousIndex = Index;
            Index = SyntaxTree.NextIndex(Index);
            return this;
        }
        private string CurrentText => SyntaxTree.TrimmedText[Index];

        private async Task<JMCSyntaxNode?> GetCurrentNodeAsync() => (await SyntaxTree.ParseAsync(Index, true)).Node;

        public async Task<bool> ExpectAsync(JMCSyntaxNodeType syntaxNodeType, bool throwError = true)
        {
            try
            {
                //parse token
                var text = SyntaxTree.TrimmedText[Index];
                var node = await GetCurrentNodeAsync();
                if (node == null) return false;
                var isMatch = node.NodeType == syntaxNodeType;

                if (!isMatch && throwError)
                {
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetIndexStartPos(Index), syntaxNodeType.ToTokenString(), node.NodeType.ToTokenString()));
                }

                return isMatch;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExpectAsync(string value, bool throwError = true)
        {
            try
            {
                //parse token
                var text = SyntaxTree.TrimmedText[Index];
                var node = await GetCurrentNodeAsync();
                if (node == null) return false;
                var isMatch = value == text;

                if (!isMatch && throwError)
                {
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetIndexStartPos(Index), value, node.NodeType.ToTokenString()));
                }

                return isMatch;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public async Task<bool> ExpectListAsync(params string[] values)
        {
            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    Next();
                    var value = values.ElementAt(i);
                    var text = SyntaxTree.TrimmedText[Index];
                    var node = (await SyntaxTree.ParseAsync(Index, true)).Node;
                    if (node == null) return false;
                    var isMatch = value == text;
                    if (!isMatch)
                    {
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetIndexStartPos(Index), value, node.NodeType.ToTokenString()));
                        return false;
                    }
                }
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public async Task<bool> ExpectListAsync(params JMCSyntaxNodeType[] nodeTypes)
        {
            try
            {
                for (var i = 0; i < nodeTypes.Length; i++)
                {
                    Next();
                    var nodeType = nodeTypes.ElementAt(i);
                    var text = SyntaxTree.TrimmedText[Index];
                    var node = await GetCurrentNodeAsync();
                    if (node == null) return false;
                    var isMatch = nodeType == node.NodeType;
                    if (!isMatch)
                    {
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetIndexStartPos(Index), nodeType.ToTokenString(), node.NodeType.ToTokenString()));
                        return false;
                    }
                }
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }

        public async Task<Tuple<bool, JMCSyntaxNodeType?>> ExpectOrAsync(params JMCSyntaxNodeType[] nodeTypes)
        {
            var node = await GetCurrentNodeAsync();
            if (node == null)
                return new(false, null);
            for (var i = 0; i < nodeTypes.Length; i++)
            {
                var type = nodeTypes[i];
                if (type == node.NodeType)
                    return new(true, type);

            }
            return new(false, null);
        }

        /// <summary>
        /// Expect <seealso cref="CurrentText"/> to be <seealso cref="int"/> or <seealso cref="bool"/>
        /// </summary>
        /// <returns><seealso cref="CurrentText"/> is <seealso cref="int"/> or <seealso cref="bool"/></returns>
        public bool ExpectInt(bool includeBool = true)
        {
            if (includeBool && CurrentText == "true" || CurrentText == "false")
                return true;

            if (CurrentText == null || CurrentText == "") return false;

            var arr = CurrentText.ToCharSpan();
            for (int i = 0; i < arr.Length; i++)
            {
                ref var s = ref arr[i];
                if ((s ^ '0') > 9)
                    return false;
            }

            return true;
        }

        public void Reset(JMCSyntaxTree syntaxTree, int startIndex = 0)
        {
            SyntaxTree = syntaxTree;
            Index = startIndex;
        }

        public void Reset(int startIndex = 0) => Index = startIndex;
    }
}
