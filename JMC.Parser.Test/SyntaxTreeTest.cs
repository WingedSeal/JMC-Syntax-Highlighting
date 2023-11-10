using JMC.Parser.JMC;
using JMC.Shared;

namespace JMC.Parser.Test
{
    public abstract class SyntaxTreeBase : IDisposable
    {
        protected SyntaxTreeBase()
        {
            var ext = new ExtensionData();
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
    public class SyntaxTreeTest : SyntaxTreeBase
    {
        [Theory]
        [MemberData(nameof(SyntaxTreeTestCase.ToOffsetTests), MemberType = typeof(SyntaxTreeTestCase))]
        public void ToOffset_Test(Range range, int offset1, int offset2)
        {
            var tree = new JMCSyntaxTree("testString;\r\ntestString2;\r\ntestString3;");
            var start = range.Start;
            var end = range.End;
            tree.ToOffset(start).Should().Be(offset1);
            tree.ToOffset(end).Should().Be(offset2);
        }

        [Theory]
        [MemberData(nameof(SyntaxTreeTestCase.ToOffsetTests), MemberType = typeof(SyntaxTreeTestCase))]
        public void ToOffset_Test2(Range range, int offset1, int offset2)
        {
            var tree = new JMCSyntaxTree("testString;\r\ntestString2;\r\ntestString3;");
            var start = offset1.ToPosition(tree.RawText);
            var end = offset2.ToPosition(tree.RawText);
            var expectRange = new Range(start, end);
            expectRange.Should().Be(range);
        }
    }
}