using JMC.Parser.JMC.Command;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private async Task<JMCParseResult> ParseCommandExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var exp = "";
            while (TrimmedText[index] != ";")
            {
                exp += SplitText[index];
                index++;
            }

            var parser = new CommandParser(exp, ToOffset(index), RawText);
            var result = parser.ParseCommand();

            //set next
            node.Next = next.Count != 0 ? next : null;

            return new(node, index, IndexToPosition(index));
        }
    }
}
