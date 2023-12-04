using JMC.Parser.JMC.Types;

namespace JMC.Parser.Test.JMC
{
    public static class JMCSyntaxTreeParserTestCase
    {
        public static readonly List<object[]> ParseTests = [
            ["++", JMCSyntaxNodeType.OpIncrement],
            ["--", JMCSyntaxNodeType.OpDecrement],

            ["+", JMCSyntaxNodeType.OpPlus],
            ["-", JMCSyntaxNodeType.OpSubtract],
            ["*", JMCSyntaxNodeType.OpMultiply],
            ["/", JMCSyntaxNodeType.OpDivide],
            ["%", JMCSyntaxNodeType.OpRemainder],

            ["-=", JMCSyntaxNodeType.OpSubtractEqual],
            ["+=", JMCSyntaxNodeType.OpPlusEqual],
            ["/=", JMCSyntaxNodeType.OpDivideEqual],
            ["*=", JMCSyntaxNodeType.OpMultiplyEqual],
            ["%=", JMCSyntaxNodeType.OpRemainderEqual],

            ["??=", JMCSyntaxNodeType.OpNullcoale],
            ["?=", JMCSyntaxNodeType.OpSuccess],
            ["><", JMCSyntaxNodeType.OpSwap],

            ["||", JMCSyntaxNodeType.CompOr],
            ["&&", JMCSyntaxNodeType.CompAnd],
            ["!", JMCSyntaxNodeType.CompNot],

            ["{", JMCSyntaxNodeType.LCP],
            ["}", JMCSyntaxNodeType.RCP],
            ["(", JMCSyntaxNodeType.LParen],
            [")", JMCSyntaxNodeType.RParen],
            [";", JMCSyntaxNodeType.Semi],
            [":", JMCSyntaxNodeType.Colon],
            ["=>", JMCSyntaxNodeType.ArrowFunction],

            [">", JMCSyntaxNodeType.GreaterThan],
            ["<", JMCSyntaxNodeType.LessThan],
            [">=", JMCSyntaxNodeType.GreaterThanEqual],
            ["<=", JMCSyntaxNodeType.LessThanEqual],
            ["=", JMCSyntaxNodeType.EqualTo],
            ["==", JMCSyntaxNodeType.Equal],
        ];
    }
}
