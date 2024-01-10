using JMC.Parser.JMC.Types;

namespace JMC.Parser.Test.JMC
{
    public static class JMCSyntaxTreeParserTestCase
    {
        //TODO: finish the test
        public static readonly object[][] ExpressionParseTests = [
            ["$test=4;",
                new List<SyntaxNode>
                {
                    new(SyntaxNodeType.Variable,"$test",[
                        new(SyntaxNodeType.EqualTo, "=",null,new(0,5,0,5),5),
                        new(SyntaxNodeType.Number, "4",null,new(0,6,0,6),6)
                    ],new(0,0,0,7),0)
                }
            ],
            ["obj:@s=4;",
                new List<SyntaxNode>
                {
                    new(SyntaxNodeType.Scoreboard,"obj:@s",[
                        new(SyntaxNodeType.EqualTo, "=",null,new(0,6,0,6),6),
                        new(SyntaxNodeType.Number, "4",null,new(0,7,0,7),7)
                    ],new(0,0,0,8),0)
                }
            ],
            ["obj:@s=$test;",
                new List<SyntaxNode>
                {
                    new(SyntaxNodeType.Scoreboard,"obj:@s",[
                        new(SyntaxNodeType.EqualTo, "=",null,new(0,6,0,6),6),
                        new(SyntaxNodeType.Variable, "$test",null,new(0,7,0,12),7)
                    ],new(0,0,0,12),0)
                }
            ],
        ];

        public static readonly object[][] BasicParseTests = [
            ["++", SyntaxNodeType.IncrementOperator],
            ["--", SyntaxNodeType.DecrementOperator],

            ["+", SyntaxNodeType.PlusOperator],
            ["-", SyntaxNodeType.SubtractOperator],
            ["*", SyntaxNodeType.MultiplyOperator],
            ["/", SyntaxNodeType.DivideOperator],
            ["%", SyntaxNodeType.RemainderOperator],

            ["-=", SyntaxNodeType.SubtractEqualOperator],
            ["+=", SyntaxNodeType.PlusEqualOperator],
            ["/=", SyntaxNodeType.DivideEqualOperator],
            ["*=", SyntaxNodeType.MultiplyEqualOperator],
            ["%=", SyntaxNodeType.RemainderEqualOperator],

            ["??=", SyntaxNodeType.NullcoaleOperator],
            ["?=", SyntaxNodeType.SuccessOperator],
            ["><", SyntaxNodeType.SwapOperator],

            ["||", SyntaxNodeType.Or],
            ["&&", SyntaxNodeType.And],
            ["!", SyntaxNodeType.Not],

            ["{", SyntaxNodeType.OpeningCurlyParenthesis],
            ["}", SyntaxNodeType.ClosingCurlyParenthesis],
            ["(", SyntaxNodeType.OpeningParenthesis],
            [")", SyntaxNodeType.ClosingParenthesis],
            [";", SyntaxNodeType.Semi],
            [":", SyntaxNodeType.Colon],
            ["=>", SyntaxNodeType.Arrow],

            [">", SyntaxNodeType.GreaterThan],
            ["<", SyntaxNodeType.LessThan],
            [">=", SyntaxNodeType.GreaterThanEqual],
            ["<=", SyntaxNodeType.LessThanEqual],
            ["=", SyntaxNodeType.EqualTo],
            ["==", SyntaxNodeType.Equal],
        ];
    }
}
