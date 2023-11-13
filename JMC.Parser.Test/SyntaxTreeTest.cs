using JMC.Parser.JMC;
using JMC.Shared;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.Test
{
    public abstract class SyntaxTreeBase : IDisposable
    {
        internal static readonly JMCSyntaxTree UtilityTestTree = new JMCSyntaxTree().InitializeAsync("testString;\r\ntestString2;\r\ntestString3;\r\ntestString4;").Result;
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
            var start = range.Start;
            var end = range.End;
            UtilityTestTree.ToOffset(start).Should().Be(offset1);
            UtilityTestTree.ToOffset(end).Should().Be(offset2);
        }

        [Theory]
        [MemberData(nameof(SyntaxTreeTestCase.ToOffsetTests), MemberType = typeof(SyntaxTreeTestCase))]
        public void ToOffset_Test2(Range range, int offset1, int offset2)
        {
            var start = offset1.ToPosition(UtilityTestTree.RawText);
            var end = offset2.ToPosition(UtilityTestTree.RawText);
            var expectRange = new Range(start, end);
            expectRange.Should().Be(range);
        }

        [Theory]
        [MemberData(nameof(SyntaxTreeTestCase.IndexToPositionTests), MemberType = typeof(SyntaxTreeTestCase))]
        public void GetIndexEndPos_Test(int index, Position pos)
        {
            var r = UtilityTestTree.GetIndexStartPos(index);
            r.Should().Be(pos);
        }

        [Theory]
        [MemberData(nameof(SyntaxTreeTestCase.GetRangeByIndexTests), MemberType = typeof(SyntaxTreeTestCase))]
        public void GetRangeByIndex_Test(int index, Range range)
        {
            var r = UtilityTestTree.GetRangeByIndex(index);
            r.Should().Be(range);
        }
    }
}