using JMC.Parser.Error.Base;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.Error
{
    internal class JMCSyntaxError(Position position, string expected, string current) : JMCBaseError($"SyntaxError: at line:{position.Line + 1} character: {position.Character + 1}; expected {expected} but got {current}", ErrorType.IDE)
    {
    }
}
