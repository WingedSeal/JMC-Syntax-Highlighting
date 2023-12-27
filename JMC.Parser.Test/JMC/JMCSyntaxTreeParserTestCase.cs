using JMC.Parser.JMC.Types;

namespace JMC.Parser.Test.JMC
{
    public static class JMCSyntaxTreeParserTestCase
    {
        //TODO: finish the test
        public static readonly object[][] ExpressionParseTests = [
            ["$test=4;",
                new List<JMCSyntaxNode>
                {
                    new(JMCSyntaxNodeType.Variable,"$test",[
                        new(JMCSyntaxNodeType.EqualTo, "=",null,new(0,5,0,5),5),
                        new(JMCSyntaxNodeType.Number, "4",null,new(0,6,0,6),6)
                    ],new(0,0,0,7),0)
                }
            ],
            ["obj:@s=4;",
                new List<JMCSyntaxNode>
                {
                    new(JMCSyntaxNodeType.Scoreboard,"obj:@s",[
                        new(JMCSyntaxNodeType.EqualTo, "=",null,new(0,6,0,6),6),
                        new(JMCSyntaxNodeType.Number, "4",null,new(0,7,0,7),7)
                    ],new(0,0,0,8),0)
                }
            ],
            ["obj:@s=$test;",
                new List<JMCSyntaxNode>
                {
                    new(JMCSyntaxNodeType.Scoreboard,"obj:@s",[
                        new(JMCSyntaxNodeType.EqualTo, "=",null,new(0,6,0,6),6),
                        new(JMCSyntaxNodeType.Variable, "$test",null,new(0,7,0,12),7)
                    ],new(0,0,0,12),0)
                }
            ],
        ];

        public static readonly object[][] BasicParseTests = [
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
            ["=>", JMCSyntaxNodeType.Arrow],

            [">", JMCSyntaxNodeType.GreaterThan],
            ["<", JMCSyntaxNodeType.LessThan],
            [">=", JMCSyntaxNodeType.GreaterThanEqual],
            ["<=", JMCSyntaxNodeType.LessThanEqual],
            ["=", JMCSyntaxNodeType.EqualTo],
            ["==", JMCSyntaxNodeType.Equal],
        ];
    }
}
