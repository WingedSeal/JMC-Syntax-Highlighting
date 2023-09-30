using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JMC.Extension.Server.Lexer.JMC;
using JMC.Extension.Server.Lexer.JMC.Types;
using Xunit;

namespace JMC.Extension.Server.Tests.Lexer
{
    //TODO: test case for commands
    public class JMCLexerTest
    {
        [Theory]
        [InlineData("function", JMCTokenType.FUNCTION)]
        [InlineData("switch", JMCTokenType.SWITCH)]
        [InlineData("case", JMCTokenType.CASE)]
        [InlineData("if", JMCTokenType.IF)]
        [InlineData("else", JMCTokenType.ELSE)]
        [InlineData("do", JMCTokenType.DO)]
        [InlineData("while", JMCTokenType.WHILE)]
        [InlineData("new", JMCTokenType.NEW)]
        [InlineData("class", JMCTokenType.CLASS)]
        public void Keyword_Tests(string text, JMCTokenType expected)
        {
            var type = JMCLexer.GetTokenType(text);
            Assert.Equal(expected, type);
        }

        [Theory]
        [MemberData(nameof(JMCLexerTestCase.StatementTestData), MemberType = typeof(JMCLexerTestCase))]
        public void Statement_Tests(string text, IEnumerable<JMCTokenType> expected)
        {
            var lexer = new JMCLexer(text);
            var tokenTypes = lexer.Tokens.Select(v => v.TokenType).ToArray();
            expected.Should().Equal(tokenTypes);
        }

        [Theory]
        [InlineData("123", JMCTokenType.NUMBER)]
        [InlineData("1.23", JMCTokenType.NUMBER)]
        [InlineData("//test comment", JMCTokenType.COMMENT)]
        [InlineData("$test", JMCTokenType.VARIABLE)]
        [InlineData("$test.get()", JMCTokenType.VARIABLE_CALL)]
        public void SpecialCases_Tests(string text, JMCTokenType expected)
        {
            var type = JMCLexer.GetTokenType(text);
            Assert.Equal(expected, type);
        }

        [Theory]
        [InlineData(@"class test{function test(){if (tag) {}}function test2(){test.test2();}}", "test2 test.test2")]
        public void FormatFunctions_Tests(string text, string expected)
        {
            var lexer = new JMCLexer(text);
            var functionDefine = lexer.FunctionDefines.ElementAt(1);
            functionDefine.Value.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(JMCLexerTestCase.FormatCommandsTest), MemberType = typeof(JMCLexerTestCase))]
        public void FormatCommands_Tests(string text, IEnumerable<JMCTokenType> expected)
        {
            var extData = new ExtensionData();
            var lexer = new JMCLexer(text);
            lexer.Tokens.Select(v => v.TokenType).Should().Equal(expected);
        }
    }
}
