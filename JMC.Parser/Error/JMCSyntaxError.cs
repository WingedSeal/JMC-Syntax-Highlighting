using JMC.Parser.Error.Base;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.Error
{
    internal class JMCSyntaxError : JMCBaseError
    {
        public JMCSyntaxError(Position position, string expected, string current) : base($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; expected {expected} but got {current}", ErrorType.IDE) { }

        public JMCSyntaxError(Position position, string text) : 
            base($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; {text}", ErrorType.IDE) { }
    }
}
