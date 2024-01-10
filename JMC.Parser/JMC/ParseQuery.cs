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
    internal class ParseQuery(SyntaxTree syntaxTree, int startIndex = 0)
    {
        private SyntaxTree SyntaxTree { get; set; } = syntaxTree;
        public int Index { get; set; } = startIndex;
        public int PreviousIndex { get; set; } = -1;
        public ParseQuery Next()
        {
            PreviousIndex = Index;
            Index = SyntaxTree.NextIndex(Index);
            return this;
        }
        private string CurrentText => SyntaxTree.TrimmedText[Index];
        private SyntaxNode? GetCurrentNode() => SyntaxTree.Parse(Index, true).Node;
        public bool Expect(SyntaxNodeType syntaxNodeType, out SyntaxNode? outNode, bool willThrowError = true)
        {
            outNode = null;
            try
            {
                //parse token
                var currentText = SyntaxTree.TrimmedText[Index];
                var currentNode = GetCurrentNode();
                if (currentNode == null) return false;
                outNode = currentNode;
                var isMatch = currentNode.NodeType == syntaxNodeType;

                if (!isMatch && willThrowError)
                    SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), syntaxNodeType.ToTokenString(), currentNode.NodeType.ToTokenString()));


                return isMatch;
            }
            catch
            {
                return false;
            }
        }
        public bool Expect(SyntaxNodeType syntaxNodeType, bool willThrowError = true) => Expect(syntaxNodeType, out _, willThrowError);
        public bool Expect(string value, out SyntaxNode? outNode, bool willThrowError = true)
        {
            outNode = null;
            try
            {
                //parse token
                var currentText = SyntaxTree.TrimmedText[Index];
                var currentNode = GetCurrentNode();
                if (currentNode == null) return false;
                outNode = currentNode;
                var isValueMatch = value == currentText;

                if (!isValueMatch && willThrowError)
                    SyntaxTree.Errors.Add(
                        new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), value, currentNode.NodeType.ToTokenString())
                    );


                return isValueMatch;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
        public bool Expect(string value, bool willThrowError = true) => Expect(value, out _, willThrowError);
        /// <summary>
        /// this will call Next() once before check
        /// </summary>
        /// <param name="outNodes"></param>
        /// <param name="willThrowError"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool ExpectList(out List<SyntaxNode> outNodes, bool willThrowError = true, params string[] values)
        {
            outNodes = [];
            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    Next();
                    var value = values.ElementAt(i);
                    var currentText = SyntaxTree.TrimmedText[Index];
                    var currentNode = SyntaxTree.Parse(Index, true).Node;
                    if (currentNode == null) return false;
                    outNodes.Add(currentNode);
                    var isValueMatch = value == currentText;
                    if (!isValueMatch && willThrowError)
                    {
                        SyntaxTree.Errors.Add(
                            new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), value, currentNode.NodeType.ToTokenString())
                        );
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
        /// <inheritdoc cref="ExpectList(out List{SyntaxNode}, bool, string[])"/>
        public bool ExpectList(bool willThrowError = true, params string[] values) => ExpectList(out _, willThrowError, values);
        /// <inheritdoc cref="ExpectList(out List{SyntaxNode}, bool, string[])"/>
        public bool ExpectList(out List<SyntaxNode> outNodes, bool throwError = true, params SyntaxNodeType[] nodeTypes)
        {
            outNodes = [];
            try
            {
                var nodeTypesSpan = nodeTypes.AsSpan();
                for (var i = 0; i < nodeTypesSpan.Length; i++)
                {
                    Next();
                    ref var nodeType = ref nodeTypesSpan[i];
                    var currentText = SyntaxTree.TrimmedText[Index];
                    var currentNode = GetCurrentNode();
                    if (currentNode == null) return false;
                    outNodes.Add(currentNode);
                    var isMatch = nodeType == currentNode.NodeType;
                    if (!isMatch && throwError)
                    {
                        SyntaxTree.Errors.Add(new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index, true), nodeType.ToTokenString(), currentNode.NodeType.ToTokenString()));
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
        /// <inheritdoc cref="ExpectList(out List{SyntaxNode}, bool, string[])"/>
        public bool ExpectList(bool willThrowError = true, params SyntaxNodeType[] nodeTypes) => ExpectList(out _, willThrowError, nodeTypes);
        public bool ExpectOr(out SyntaxNode? outNode, params SyntaxNodeType[] nodeTypes)
        {
            var currentNode = GetCurrentNode();
            outNode = null;
            if (currentNode == null)
            {
                return false;
            }
            var nodeTypesSpan = nodeTypes.AsSpan();
            for (var i = 0; i < nodeTypesSpan.Length; i++)
            {
                ref var currentType = ref nodeTypesSpan[i];
                if (currentType == currentNode.NodeType)
                {
                    outNode = currentNode;
                    return true;
                }

            }
            return false;
        }
        public bool ExpectOr(params SyntaxNodeType[] nodeTypes) => ExpectOr(out _, nodeTypes);
        /// <summary>
        /// Expect <seealso cref="CurrentText"/> to be <seealso cref="int"/> or <seealso cref="bool"/>
        /// </summary>
        /// <returns><seealso cref="CurrentText"/> is <seealso cref="int"/> or <seealso cref="bool"/></returns>
        public bool ExpectInt(bool includeBool = true)
        {
            if (includeBool && CurrentText == "true" || CurrentText == "false")
                return true;

            if (CurrentText == null || CurrentText == "") return false;

            var currentTextCharSpan = CurrentText.ToCharSpan();
            for (int i = 0; i < currentTextCharSpan.Length; i++)
            {
                ref var currentChar = ref currentTextCharSpan[i];
                if ((currentChar ^ '0') > 9)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>start at '{'</remarks>
        public bool ExpectJSON(out string jsonString)
        {
            jsonString = CurrentText;
            var parenthesisCounter = 1;
            while (Index < SyntaxTree.TrimmedText.Length)
            {
                Next();
                parenthesisCounter += CurrentText switch
                {
                    "{" => 1,
                    "}" => -1,
                    _ => 0
                };
                jsonString += CurrentText;
                if (parenthesisCounter == 0) break;
            }
            try
            {
                JsonConvert.DeserializeObject(jsonString);
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
        public bool ExpectJSONList(out string jsonArrayString)
        {
            jsonArrayString = CurrentText;
            var parenthesisCounter = 1;
            while (Index < SyntaxTree.TrimmedText.Length)
            {
                Next();
                parenthesisCounter += CurrentText switch
                {
                    "[" => 1,
                    "]" => -1,
                    _ => 0
                };
                jsonArrayString += CurrentText;
                if (parenthesisCounter == 0) break;
            }
            try
            {
                JsonConvert.DeserializeObject<List<object>>(jsonArrayString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        public bool ExpectArrowFunction(out SyntaxNode? outNode)
        {
            outNode = null;
            var match = Expect(SyntaxNodeType.OpeningParenthesis) && ExpectList(true, SyntaxNodeType.ClosingParenthesis, SyntaxNodeType.Arrow);
            if (!match) return false;
            Next();
            var block = SyntaxTree.ParseBlock(Index);
            outNode = block.Node;
            return true;
        }
        public bool ExpectIntRange()
        {
            var currentText = CurrentText;
            try
            {
                var splitText = currentText.Split("..");
                var head = splitText.First();
                var tail = splitText.Last();

                var isHeadInt = int.TryParse(head, out _) || head == string.Empty && tail != string.Empty;
                var isTailInt = int.TryParse(tail, out _) || tail == string.Empty && head != string.Empty;

                if (isHeadInt && isTailInt)
                    return true;
                else
                {
                    SyntaxTree.Errors.Add(
                        new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index), SyntaxNodeType.IntRange.ToTokenString(), currentText)
                    );
                    return false;
                }
            }
            catch
            {
                SyntaxTree.Errors.Add(
                    new JMCSyntaxError(SyntaxTree.GetRangeByIndex(Index), SyntaxNodeType.IntRange.ToTokenString(), currentText)
                );
                return false;
            }
        }
        public void Reset(SyntaxTree syntaxTree, int startIndex = 0)
        {
            SyntaxTree = syntaxTree;
            Index = startIndex;
        }
        public void Reset(int startIndex = 0) => Index = startIndex;
    }
}
