using JMC.Parser.JMC;
using JMC.Shared;
using Xunit.Abstractions;

namespace JMC.Parser.Test.JMC
{
    public abstract class JMCSyntaxTreeTestBase : IDisposable
    {
        internal static readonly SyntaxTree UtilityTestTree = new SyntaxTree().InitializeAsync("testString;\r\ntestString2;\r\ntestString3;\r\ntestString4;").Result;
        internal static readonly SyntaxTree ParserBaseTree = new();
        protected readonly ITestOutputHelper output;

        protected JMCSyntaxTreeTestBase(ITestOutputHelper output)
        {
            var ext = new ExtensionData();
            this.output = output;
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}