using JMC.Parser.JMC;
using JMC.Shared;

namespace JMC.Parser.Test.JMC
{
    public abstract class JMCSyntaxTreeTestBase : IDisposable
    {
        internal static readonly JMCSyntaxTree UtilityTestTree = new JMCSyntaxTree().InitializeAsync("testString;\r\ntestString2;\r\ntestString3;\r\ntestString4;").Result;
        internal static readonly JMCSyntaxTree ParserBaseTree = new();
        protected JMCSyntaxTreeTestBase()
        {
            var ext = new ExtensionData();
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}