using JMC.Parser.JMC.Error.Base;
using JMC.Parser.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC.Error
{
    internal class JMCSyntaxError : JMCBaseError
    {
        public JMCSyntaxError(Position position, string current, params string[] expected) : base($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; expected [{string.Join(',', expected)}] but got {current}", ErrorType.IDE) { }

        public JMCSyntaxError(Position position, string current, params JMCSyntaxNodeType[] expected) : 
            base($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; expected [{string.Join(',', expected.Select(v => v.ToTokenString()))}] but got {current}", ErrorType.IDE) { }

        public JMCSyntaxError(Position position, string expected, string current) : base($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; expected {expected} but got {current}", ErrorType.IDE) { }

        public JMCSyntaxError(Position position, string text) :
            base($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; {text}", ErrorType.IDE)
        { }
    }
}
