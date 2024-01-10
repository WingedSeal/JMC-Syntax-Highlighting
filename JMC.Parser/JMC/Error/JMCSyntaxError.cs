using JMC.Parser.JMC.Error.Base;
using JMC.Parser.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC.Error
{
    internal class JMCSyntaxError : JMCBaseError
    {
        public JMCSyntaxError(Range range, string current, params string[] expected) : base($"SyntaxError: at line:{range.Start.Line + 1} character: {range.Start.Character + 1}; expected [{string.Join(',', expected)}] but got {current}", ErrorType.IDE, range) { }

        public JMCSyntaxError(Range range, string current, params SyntaxNodeType[] expected) : 
            base($"SyntaxError: at line:{range.Start.Line + 1} character: {range.Start.Character + 1}; expected [{string.Join(',', expected.Select(v => v.ToTokenString()))}] but got {current}", ErrorType.IDE, range) { }

        public JMCSyntaxError(Range range, string expected, string current) : base($"SyntaxError: at line:{range.Start.Line + 1} character: {range.Start.Character + 1}; expected {expected} but got {current}", ErrorType.IDE, range) { }

        public JMCSyntaxError(Range range, string text) :
            base($"SyntaxError: at line:{range.Start.Line + 1} character: {range.Start.Character + 1}; {text}", ErrorType.IDE, range)
        { }
    }
}
