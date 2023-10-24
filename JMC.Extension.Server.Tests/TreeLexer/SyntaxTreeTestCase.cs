using JMC.Parser.JMC;

namespace JMC.Extension.Server.Tests.TreeLexer
{
    public class SyntaxTreeTestCase
    {
        public static readonly IEnumerable<object[]> PraseTest = new object[][]
        {
            [
                //import & start spaces
                " import \"test/test\\*\";", 
                new JMCSyntaxNodeType[]
                {
                    JMCSyntaxNodeType.IMPORT, JMCSyntaxNodeType.STRING
                },
                new Range[]
                {
                    new(0, 1, 0, 21), new(0, 8, 0, 21)
                }
            ],
            [
                //class
                "class test {}",
                new JMCSyntaxNodeType[]
                {
                    JMCSyntaxNodeType.CLASS
                },
                new Range[]
                {
                    new(0, 11, 0, 12) 
                }
            ],
            [
                //function
                "function test() {}",
                new JMCSyntaxNodeType[]
                {
                    JMCSyntaxNodeType.FUNCTION
                },
                new Range[]
                {
                    new(0, 16, 0, 17)
                }
            ],
            [
                //class & function
                "class test {function test(){}}",
                new JMCSyntaxNodeType[]
                {
                    JMCSyntaxNodeType.CLASS ,JMCSyntaxNodeType.FUNCTION
                },
                new Range[]
                {
                    new(0, 11, 0, 29), new(0, 27, 0, 28)
                }
            ]
        };
    }
}
