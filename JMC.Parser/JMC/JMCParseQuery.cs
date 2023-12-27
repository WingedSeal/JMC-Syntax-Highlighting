using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;
using Newtonsoft.Json;

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

        private JMCSyntaxNode? GetCurrentNode() => SyntaxTree.Parse(Index, true).Node;

        public bool Expect(JMCSyntaxNodeType syntaxNodeType, out JMCSyntaxNode? syntaxNode, bool throwError = true)
        {
            syntaxNode = null;
            try
            {
                //parse token
                var text = SyntaxTree.TrimmedText[Index];
                var node = GetCurrentNode();
                if (node == null) return false;
                syntaxNode = node;
                var isMatch = node.NodeType == syntaxNodeType;

                if (!isMatch && throwError)
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), syntaxNodeType.ToTokenString(), node.NodeType.ToTokenString()));


                return isMatch;
            }
            catch
            {
                return false;
            }
        }
        public bool Expect(JMCSyntaxNodeType syntaxNodeType, bool throwError = true) => Expect(syntaxNodeType, out _, throwError);
        public bool Expect(string value, out JMCSyntaxNode? syntaxNode, bool throwError = true)
        {
            syntaxNode = null;
            try
            {
                //parse token
                var text = SyntaxTree.TrimmedText[Index];
                var node = GetCurrentNode();
                if (node == null) return false;
                syntaxNode = node;
                var isMatch = value == text;

                if (!isMatch && throwError)
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), value, node.NodeType.ToTokenString()));


                return isMatch;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        public bool Expect(string value, bool throwError = true) => Expect(value, out _, throwError);
        public bool ExpectList(out List<JMCSyntaxNode> nodes, bool throwError = true, params string[] values)
        {
            nodes = [];
            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    Next();
                    var value = values.ElementAt(i);
                    var text = SyntaxTree.TrimmedText[Index];
                    var node = (SyntaxTree.Parse(Index, true)).Node;
                    if (node == null) return false;
                    nodes.Add(node);
                    var isMatch = value == text;
                    if (!isMatch)
                    {
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), value, node.NodeType.ToTokenString()));
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
        public bool ExpectList(bool throwError = true, params string[] values) => ExpectList(out _, throwError, values);
        public bool ExpectList(out List<JMCSyntaxNode> nodes, bool throwError = true, params JMCSyntaxNodeType[] nodeTypes)
        {
            nodes = [];
            try
            {
                var arr = nodeTypes.AsSpan();
                for (var i = 0; i < arr.Length; i++)
                {
                    Next();
                    ref var nodeType = ref arr[i];
                    var text = SyntaxTree.TrimmedText[Index];
                    var node = GetCurrentNode();
                    if (node == null) return false;
                    nodes.Add(node);
                    var isMatch = nodeType == node.NodeType;
                    if (!isMatch && throwError)
                    {
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), nodeType.ToTokenString(), node.NodeType.ToTokenString()));
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
        public bool ExpectList(bool throwError = true, params JMCSyntaxNodeType[] nodeTypes) => ExpectList(out _, throwError, nodeTypes);

        public bool ExpectOr(out JMCSyntaxNode? syntaxNode, params JMCSyntaxNodeType[] nodeTypes)
        {
            var node = GetCurrentNode();
            syntaxNode = null;
            if (node == null)
            {
                return false;
            }
            var arr = nodeTypes.AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var type = ref arr[i];
                if (type == node.NodeType)
                {
                    syntaxNode = node;
                    return true;
                }

            }
            return false;
        }
        public bool ExpectOr(params JMCSyntaxNodeType[] nodeTypes) => ExpectOr(out _, nodeTypes);
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>start at '{'</remarks>
        public bool ExpectJSON(out string JString)
        {
            JString = CurrentText;
            var counter = 1;
            while (Index < SyntaxTree.TrimmedText.Length)
            {
                Next();
                counter += CurrentText switch
                {
                    "{" => 1,
                    "}" => -1,
                    _ => 0
                };
                JString += CurrentText;
                if (counter == 0) break;
            }
            try
            {
                JsonConvert.DeserializeObject(JString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>start at '['</remarks>
        public bool ExpectJSONList(out string LString)
        {
            LString = CurrentText;
            var counter = 1;
            while (Index < SyntaxTree.TrimmedText.Length)
            {
                Next();
                counter += CurrentText switch
                {
                    "[" => 1,
                    "]" => -1,
                    _ => 0
                };
                LString += CurrentText;
                if (counter == 0) break;
            }
            try
            {
                JsonConvert.DeserializeObject<List<object>>(LString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        public bool ExpectArrowFunction(out JMCSyntaxNode? syntaxNode)
        {
            syntaxNode = null;
            var match = Expect(JMCSyntaxNodeType.LParen) && ExpectList(true, JMCSyntaxNodeType.RParen, JMCSyntaxNodeType.Arrow);
            if (!match) return false;
            Next();
            var block = SyntaxTree.ParseBlock(Index);
            syntaxNode = block.Node;
            return true;
        }
        public bool ExpectIntRange()
        {
            var text = CurrentText;
            try
            {
                var split = text.Split("..");
                var head = split.First();
                var tail = split.Last();

                var isHeadInt = int.TryParse(head, out _) || head == string.Empty && tail != string.Empty;
                var isTailInt = int.TryParse(tail, out _) || tail == string.Empty && head != string.Empty;

                if (isHeadInt && isTailInt)
                    return true;
                else
                {
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index), JMCSyntaxNodeType.IntRange.ToTokenString(), text));
                    return false;
                }
            }
            catch
            {
                SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index), JMCSyntaxNodeType.IntRange.ToTokenString(), text));
                return false;
            }
        }

        public void Reset(JMCSyntaxTree syntaxTree, int startIndex = 0)
        {
            SyntaxTree = syntaxTree;
            Index = startIndex;
        }

        public void Reset(int startIndex = 0) => Index = startIndex;
    }
}
