using JMC.Parser.JMC.Types;
using Newtonsoft.Json;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace JMC.Parser.Test.JMC
{
    public class JMCSyntaxTreeParserTest(ITestOutputHelper testOutputHelper) : JMCSyntaxTreeTestBase(testOutputHelper)
    {
        [Theory]
        [MemberData(nameof(JMCSyntaxTreeParserTestCase.BasicParseTests), MemberType = typeof(JMCSyntaxTreeParserTestCase))]
        public async Task BasicParse_Test(string text, JMCSyntaxNodeType type)
        {
            var tree = await ParserBaseTree.InitializeAsync(text);
            tree.Errors.Should().BeEmpty();

            var expected = new JMCSyntaxNode()
            {
                NodeType = type,
                Range = new(0, 0, 0, text.Length - 1),
                Value = text,
                Offset = 0,
                Next = null
            };

            Assert.Equivalent(expected, tree.FlattenedNodes.First());
        }

        [Theory]
        [MemberData(nameof(JMCSyntaxTreeParserTestCase.ExpressionParseTests), MemberType = typeof(JMCSyntaxTreeParserTestCase))]
        internal async Task ExpressionParse_Test(string text, List<JMCSyntaxNode> expectedNodes)
        {
            var tree = await ParserBaseTree.InitializeAsync(text);
            tree.Errors.Should().BeEmpty();
            try
            {
                tree.Nodes.Should().BeEquivalentTo(expectedNodes);
            }
            catch (XunitException)
            {
                var json = JsonConvert.SerializeObject(tree.Nodes, Formatting.Indented);
                output.WriteLine(json);
            }
            tree.Nodes.Should().BeEquivalentTo(expectedNodes);
        }
    }
}
