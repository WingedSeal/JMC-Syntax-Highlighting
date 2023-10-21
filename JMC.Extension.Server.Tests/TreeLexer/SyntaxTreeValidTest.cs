using FluentAssertions;
using JMC.Extension.Server.Lexer;
using JMC.Extension.Server.Lexer.JMC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyntaxTree = JMC.Extension.Server.Lexer.SyntaxTree;

namespace JMC.Extension.Server.Tests.TreeLexer
{
    public class SyntaxTreeValidTest
    {
        [Theory(DisplayName = "Parsing")]
        [MemberData(nameof(SyntaxTreeTestCase.PraseTest), MemberType = typeof(SyntaxTreeTestCase))]
        public void Parsing_Test(string text, IEnumerable<SyntaxNodeType> nodeTypes, IEnumerable<Range> ranges)
        {
            var tree = new SyntaxTree(text);
            tree.Errors.Should().BeEmpty();
            var nodes = tree.GetFlattenNodes();
            nodes.Select(v => v.NodeType).Should().Equal(nodeTypes);
            nodes.Select(v => v.Range).Should().Equal(ranges);
        }
    }
}
