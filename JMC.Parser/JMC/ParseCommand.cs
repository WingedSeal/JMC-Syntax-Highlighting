using JMC.Parser.JMC.Command;
using JMC.Parser.JMC.Error;
using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class SyntaxTree
    {
        /// <summary>
        /// Parse a command
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ParseResult ParseCommandExpression(int index)
        {
            var node = new SyntaxNode();
            var next = new List<SyntaxNode>();
            var comamndString = "";
            var startOffset = ToOffset(index);
            while (TrimmedText[index] != ";")
            {
                comamndString += SplitText[index];
                index++;
            }
            var parser = new CommandParser(comamndString, startOffset, RawText);
            var result = parser.ParseCommand();
            next.AddRange(result);
            Errors.AddRange(parser.Errors.Select(v =>
                {
                    var startPos = (v.Key + startOffset).ToPosition(RawText);
                    var endPos = (v.Key + startOffset + RawText.Length).ToPosition(RawText);
                    var error = new JMCSyntaxError(new(startPos, endPos), v.Value);
                    return error;
                }
            ));

            var startPos = startOffset.ToPosition(RawText);
            var endPos = GetIndexStartPos(index);
            var range = new Range(startPos, endPos);
            //set next
            node.Next = next.Count != 0 ? next : null;
            node.Range = range;
            node.NodeType = SyntaxNodeType.Command;

            return new(node, index);
        }
    }
}
