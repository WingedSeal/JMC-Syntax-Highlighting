using JMC.Extension.Server.Lexer.JMC.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Tests.Lexer
{
    public class JMCLexerTestCase
    {
        public static IEnumerable<object[]> StatementTestData => new List<object[]>
        {
            //function test
            new object[]
            {
                "function test() {}", new JMCTokenType[]
                {
                    JMCTokenType.FUNCTION, JMCTokenType.LITERAL,
                    JMCTokenType.LPAREN, JMCTokenType.RPAREN,
                    JMCTokenType.LCP, JMCTokenType.RCP,
                }
            },
            //class test
            new object[]
            {
                "class name.name {}", new JMCTokenType[]
                {
                    JMCTokenType.CLASS, JMCTokenType.LITERAL, JMCTokenType.LCP, JMCTokenType.RCP
                }
            },
            //switch test
            new object[]
            {
                "switch ($test) {case 1: test();}", new JMCTokenType[]
                {
                    JMCTokenType.SWITCH, JMCTokenType.LPAREN, JMCTokenType.VARIABLE, JMCTokenType.RPAREN,
                    JMCTokenType.LCP,
                    JMCTokenType.CASE, JMCTokenType.NUMBER, JMCTokenType.COLON,
                    JMCTokenType.LITERAL, JMCTokenType.LPAREN, JMCTokenType.RPAREN, JMCTokenType.SEMI,
                    JMCTokenType.RCP,
                }
            },
            //comment test
            new object[]
            {
                "//test comment\r\ntest();", new JMCTokenType[]
                {
                    JMCTokenType.COMMENT, JMCTokenType.LITERAL, JMCTokenType.LPAREN, JMCTokenType.RPAREN, JMCTokenType.SEMI
                }
            }
            //new object[]
            //{
            //    "if ($test == 3) {} else {}", new JMCTokenType[]
            //    {
            //        JMCTokenType.IF,
            //        JMCTokenType.LPAREN, JMCTokenType.VARIABLE, JMCTokenType.OP_EQUAL, JMCTokenType.NUMBER ,JMCTokenType.RPAREN,
            //        JMCTokenType.LCP, JMCTokenType.RCP,
            //        JMCTokenType.ELSE,
            //        JMCTokenType.LCP, JMCTokenType.RCP
            //    }
            //}
        };
        public static IEnumerable<object[]> FormatCommandsTest = new List<object[]>()
        {
            new object[]
            {
                "teleport @s ~ ~ ~;",
                new JMCTokenType[]
                {
                    JMCTokenType.COMMAND_LITERAL,JMCTokenType.COMMAND_SELECTOR ,JMCTokenType.COMMAND_VEC2, JMCTokenType.SEMI
                }
            }
        };
    }
}
