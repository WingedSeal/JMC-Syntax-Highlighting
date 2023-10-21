using JMC.Extension.Server.Helper;
using JMC.Extension.Server.Lexer.Error.Base;
using JMC.Extension.Server.Lexer.JMC;

namespace JMC.Extension.Server.Lexer.JMC
{
    internal partial class JMCSyntaxTree
    {
        private async Task<JMCParseResult> ParseDoAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseWhileAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseForAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseSwitchAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }

        private async Task<JMCParseResult> ParseIfAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(null, index, ToOffset(index).ToPosition(RawText));
        }
    }
}
