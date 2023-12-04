using JMC.Parser.JMC.Command;
using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        /// <summary>
        /// Parse a command
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private JMCParseResult ParseCommandExpression(int index)
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
            Errors.AddRange(parser.Errors.Select(v =>
                {
                    var start = (v.Key + startOffset).ToPosition(RawText);
                    var end = (v.Key + startOffset + RawText.Length).ToPosition(RawText);
                    var error = new JMCSyntaxError(new(start, end), v.Value);
                    return error;
                }
            ));

            var start = startOffset.ToPosition(RawText);
            var end = GetIndexStartPos(index);
            var range = new Range(start, end);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = range;
            node.NodeType = JMCSyntaxNodeType.Command;

            return new(node, index);
        }
    }
}
