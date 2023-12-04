using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.Test.JMC
{
    public static class JMCSyntaxTreeUtilityTestCase
    {
        public static readonly List<object[]> ToOffsetTests = [
            [new Range(0, 0, 0, 3), 0, 3],
            [new Range(1, 0, 2, 4), 13, 31],
            [new Range(1, 0, 2, 0), 13, 27]
        ];

        public static readonly List<object[]> GetRangeByIndexTests = [
            [0, new Range(0, 0, 0, 9)],
            [4, new Range(1, 0, 1, 10)],
            [8, new Range(2, 0, 2, 10)],
        ];

        public static readonly List<object[]> IndexToPositionTests = [
            [0, new Position(0, 0)],
            [4, new Position(1, 0)],
        ];
    }
}