using JMC.Parser.Error.Base;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.Error
{
    internal class SyntaxError(Position position, string expected, string current) : JMCBaseError($"SyntaxError: at line:{position.Line} character: {position.Character}; expected {expected} but got {current}", ErrorType.IDE)
    {
    }
}
