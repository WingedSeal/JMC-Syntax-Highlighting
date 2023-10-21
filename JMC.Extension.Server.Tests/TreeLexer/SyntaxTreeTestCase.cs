using JMC.Extension.Server.Lexer.JMC;

namespace JMC.Extension.Server.Tests.TreeLexer
{
    public class SyntaxTreeTestCase
    {
        public static readonly IEnumerable<object[]> PraseTest = new object[][]
        {
            [
                //import & start spaces
                " import \"test/test\\*\";", 
                new SyntaxNodeType[]
                {
                    SyntaxNodeType.IMPORT, SyntaxNodeType.STRING
                },
                new Range[]
                {
                    new(0, 1, 0, 21), new(0, 8, 0, 21)
                }
            ],
            [
                //class
                "class test {}",
                new SyntaxNodeType[]
                {
                    SyntaxNodeType.CLASS
                },
                new Range[]
                {
                    new(0, 11, 0, 12) 
                }
            ],
            [
                //function
                "function test() {}",
                new SyntaxNodeType[]
                {
                    SyntaxNodeType.FUNCTION
                },
                new Range[]
                {
                    new(0, 16, 0, 17)
                }
            ],
            [
                //class & function
                "class test {function test(){}}",
                new SyntaxNodeType[]
                {
                    SyntaxNodeType.CLASS ,SyntaxNodeType.FUNCTION
                },
                new Range[]
                {
                    new(0, 11, 0, 29), new(0, 27, 0, 28)
                }
            ]
        };
    }
}
