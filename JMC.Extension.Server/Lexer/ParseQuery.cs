using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.Error;
using JMC.Extension.Server.Lexer.Error.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JMC.Extension.Server.Lexer
{
    internal class ParseQuery(SyntaxTree syntaxTree, int startIndex = 0)
    {
        private SyntaxTree SyntaxTree { get; set; } = syntaxTree;
        public int Index { get; set; } = startIndex;
        public ParseQuery Next()
        {
            Index++;
            return this;
        }

        public async Task<bool> Expect(SyntaxNodeType syntaxNodeType)
        {
            try
            {
                //parse token
                var text = SyntaxTree.TrimmedText[Index];
                var node = (await SyntaxTree.ParseAsync(Index, true)).Node;
                if (node == null) return false;
                var isMatch = node.NodeType == syntaxNodeType;

                if (!isMatch)
                {
                    SyntaxTree.Errors.Add(new SyntaxError(Index.ToPosition(SyntaxTree.RawText), syntaxNodeType.ToTokenString(), node.NodeType.ToTokenString()));
                }

                return isMatch;
            } 
            catch
            {
                return false;
            }
        }

    }
}
