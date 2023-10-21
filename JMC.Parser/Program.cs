using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using JMC.Parser.Content;

namespace JMC.Parser
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var content = "$test;";
            var stream = new AntlrInputStream(content);
            var lexer = new JMCLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new JMCParser(tokens);
            var context = parser.program();
            Console.WriteLine(context.ToStringTree());
        }
    }
}
