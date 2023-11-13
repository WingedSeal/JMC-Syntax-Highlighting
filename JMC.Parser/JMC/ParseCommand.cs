using JMC.Parser.Error;
using JMC.Parser.JMC.Command;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        /// <summary>
        /// Parse a command
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private async Task<JMCParseResult> ParseCommandExpressionAsync(int index)
        {
            var node = new JMCSyntaxNode();
            var next = new List<JMCSyntaxNode>();
            var exp = "";
            var startOffset = ToOffset(index);
            while (TrimmedText[index] != ";")
            {
                exp += SplitText[index];
                index++;
            }
            var parser = new CommandParser(exp, startOffset, RawText);
            var result = parser.ParseCommand();
            next.AddRange(result);
            Errors.AddRange(parser.Errors.Select(v => new JMCSyntaxError((v.Key + startOffset).ToPosition(RawText), v.Value)));

            var start = startOffset.ToPosition(RawText);
            var end = GetIndexStartPos(index);
            var range = new Range(start, end);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = range;
            node.NodeType = JMCSyntaxNodeType.COMMAND;

            return new(node, index, GetIndexStartPos(index));
        }
    }
}
