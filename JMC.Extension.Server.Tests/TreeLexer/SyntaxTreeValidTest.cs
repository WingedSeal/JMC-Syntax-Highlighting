using FluentAssertions;
using JMC.Parser.JMC;

namespace JMC.Extension.Server.Tests.TreeLexer
{
    public class SyntaxTreeValidTest
    {
        [Theory(DisplayName = "Parsing")]
        [MemberData(nameof(SyntaxTreeTestCase.PraseTest), MemberType = typeof(SyntaxTreeTestCase))]
        public void Parsing_Test(string text, IEnumerable<JMCSyntaxNodeType> nodeTypes, IEnumerable<Range> ranges)
        {
            var tree = new JMCSyntaxTree(text);
            tree.Errors.Should().BeEmpty();
            var nodes = tree.GetFlattenNodes();
            nodes.Select(v => v.NodeType).Should().Equal(nodeTypes);
            nodes.Select(v => v.Range).Should().Equal(ranges);
        }
    }
}
