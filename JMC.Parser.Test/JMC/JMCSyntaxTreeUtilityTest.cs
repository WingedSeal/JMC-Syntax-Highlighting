using JMC.Parser.JMC;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Xunit.Abstractions;

namespace JMC.Parser.Test.JMC
{
    public class JMCSyntaxTreeUtilityTest(ITestOutputHelper testOutputHelper) : JMCSyntaxTreeTestBase(testOutputHelper)
    {
        [Theory]
        [MemberData(nameof(JMCSyntaxTreeUtilityTestCase.ToOffsetTests), MemberType = typeof(JMCSyntaxTreeUtilityTestCase))]
        public void ToOffset_Test(Range range, int offset1, int offset2)
        {
            var start = range.Start;
            var end = range.End;
            UtilityTestTree.ToOffset(start).Should().Be(offset1);
            UtilityTestTree.ToOffset(end).Should().Be(offset2);
        }

        [Theory]
        [MemberData(nameof(JMCSyntaxTreeUtilityTestCase.ToOffsetTests), MemberType = typeof(JMCSyntaxTreeUtilityTestCase))]
        public void ToOffset_Test2(Range range, int offset1, int offset2)
        {
            var start = offset1.ToPosition(UtilityTestTree.RawText);
            var end = offset2.ToPosition(UtilityTestTree.RawText);
            var expectRange = new Range(start, end);
            expectRange.Should().Be(range);
        }

        [Theory]
        [MemberData(nameof(JMCSyntaxTreeUtilityTestCase.IndexToPositionTests), MemberType = typeof(JMCSyntaxTreeUtilityTestCase))]
        public void GetIndexEndPos_Test(int index, Position pos)
        {
            var r = UtilityTestTree.GetIndexStartPos(index);
            r.Should().Be(pos);
        }

        [Theory]
        [MemberData(nameof(JMCSyntaxTreeUtilityTestCase.GetRangeByIndexTests), MemberType = typeof(JMCSyntaxTreeUtilityTestCase))]
        public void GetRangeByIndex_Test(int index, Range range)
        {
            var r = UtilityTestTree.GetRangeByIndex(index);
            r.Should().Be(range);
        }
    }
}