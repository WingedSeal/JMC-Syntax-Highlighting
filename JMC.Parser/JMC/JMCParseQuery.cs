using JMC.Parser.Error;

namespace JMC.Parser.JMC
{
    internal class JMCParseQuery(JMCSyntaxTree syntaxTree, int startIndex = 0)
    {
        private JMCSyntaxTree SyntaxTree { get; set; } = syntaxTree;
        public int Index { get; set; } = startIndex;
        public JMCParseQuery Next()
        {
            Index++;
            Index = SyntaxTree.SkipToValue(Index);
            return this;
        }
        private string CurrentText => SyntaxTree.TrimmedText[Index];

        private async Task<JMCSyntaxNode?> GetCurrentNodeAsync() => (await SyntaxTree.ParseAsync(Index, true)).Node;

        public async Task<bool> ExpectAsync(JMCSyntaxNodeType syntaxNodeType, bool throwError = true)
        {
            try
            {
                if (syntaxNodeType != JMCSyntaxNodeType.VEC2 && syntaxNodeType != JMCSyntaxNodeType.VEC3)
                {
                    //parse token
                    var text = SyntaxTree.TrimmedText[Index];
                    var node = await GetCurrentNodeAsync();
                    if (node == null) return false;
                    var isMatch = node.NodeType == syntaxNodeType;

                    if (!isMatch && throwError)
                    {
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.IndexToPosition(Index), syntaxNodeType.ToTokenString(), node.NodeType.ToTokenString()));
                    }

                    return isMatch;
                }
                else
                {
                    var node = await GetCurrentNodeAsync();
                    if (node == null) return false;
                    var startType = node.NodeType;

                    var counter = syntaxNodeType == JMCSyntaxNodeType.VEC2 ? 1 : 0;
                    var loop = syntaxNodeType == JMCSyntaxNodeType.VEC2 ? 2 : 0;
                    var previousNode = node;
                    while (counter != 2)
                    {
                        if (loop >= 6) break;
                        Next();
                        var currentNode = await GetCurrentNodeAsync();
                        if (currentNode == null) return false;
                        var nodeType = currentNode.NodeType;
                        if (nodeType == JMCSyntaxNodeType.NUMBER && (startType == JMCSyntaxNodeType.TILDE || startType == JMCSyntaxNodeType.CARET))
                            counter--;
                        var result = true;
                        //~
                        if (startType == JMCSyntaxNodeType.TILDE && previousNode.NodeType != JMCSyntaxNodeType.TILDE)
                            result = await ExpectAsync(JMCSyntaxNodeType.TILDE);

                        else if (startType == JMCSyntaxNodeType.TILDE && previousNode.NodeType == JMCSyntaxNodeType.TILDE)
                            result = await ExpectOrAsync(JMCSyntaxNodeType.TILDE, JMCSyntaxNodeType.NUMBER);

                        //^
                        else if (startType == JMCSyntaxNodeType.CARET && previousNode.NodeType != JMCSyntaxNodeType.CARET)
                            result = await ExpectAsync(JMCSyntaxNodeType.CARET);

                        else if (startType == JMCSyntaxNodeType.CARET && previousNode.NodeType == JMCSyntaxNodeType.CARET)
                            result = await ExpectOrAsync(JMCSyntaxNodeType.CARET, JMCSyntaxNodeType.NUMBER);

                        //number
                        else if (startType == JMCSyntaxNodeType.NUMBER)
                            result = await ExpectAsync(JMCSyntaxNodeType.NUMBER);

                        if (!result) break;

                        counter++;
                        loop++;
                        previousNode = currentNode;
                    }

                    //parse if next is number
                    if (startType == JMCSyntaxNodeType.TILDE || startType == JMCSyntaxNodeType.CARET)
                    {
                        var c = Index;
                        Next();
                        var n = await GetCurrentNodeAsync();
                        if (n == null || n.NodeType != JMCSyntaxNodeType.NUMBER)
                            Index = c;
                    }

                    if (loop >= 6)
                        SyntaxTree.Errors.Add(new JMCSyntaxError(previousNode.Range.Start, "VECTOR", previousNode.Value));

                    return true;
                }

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
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.IndexToPosition(Index), value, node.NodeType.ToTokenString()));
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
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.IndexToPosition(Index), value, node.NodeType.ToTokenString()));
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
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.IndexToPosition(Index), nodeType.ToTokenString(), node.NodeType.ToTokenString()));
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

        public async Task<bool> ExpectOrAsync(params JMCSyntaxNodeType[] nodeTypes)
        {
            var node = await GetCurrentNodeAsync();
            if (node == null)
                return false;
            for (var i = 0; i < nodeTypes.Length; i++)
            {
                var type = nodeTypes[i];
                if (type == node.NodeType)
                    return true;
            }
            return false;
        }


        public bool ExpectInt()
        {
            try
            {
                if (CurrentText == "true" || CurrentText == "false")
                    return true;

                var check = int.Parse(CurrentText);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Reset(JMCSyntaxTree syntaxTree, int startIndex = 0)
        {
            SyntaxTree = syntaxTree;
            Index = startIndex;
        }
    }
}
