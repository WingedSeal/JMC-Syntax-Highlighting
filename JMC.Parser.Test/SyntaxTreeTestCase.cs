namespace JMC.Parser.Test
{
    public static class SyntaxTreeTestCase
    {
        public static readonly List<object[]> ToOffsetTests = [
            [new Range(0, 0, 0, 3), 0, 3],
            [new Range(1, 0, 2, 4), 13, 31],
            [new Range(1, 0, 2, 0), 13, 27]
        ];
    }
}