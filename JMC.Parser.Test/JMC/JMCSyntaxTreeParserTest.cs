using JMC.Parser.JMC.Types;

namespace JMC.Parser.Test.JMC
{
    public class JMCSyntaxTreeParserTest : JMCSyntaxTreeTestBase
    {
        [Theory]
        [MemberData(nameof(JMCSyntaxTreeParserTestCase.ParseTests), MemberType = typeof(JMCSyntaxTreeParserTestCase))]
        public async Task BasicParse_Test(string text, JMCSyntaxNodeType type)
        {
            var tree = await ParserBaseTree.InitializeAsync(text);
            tree.Errors.Should().BeEmpty();

            var expected = new JMCSyntaxNode()
            {
                NodeType = type,
                Range = new(0, 0, 0, text.Length - 1),
                Value = text
            };

            Assert.Equivalent(expected, tree.FlattenedNodes.First());
        }
    }
}
