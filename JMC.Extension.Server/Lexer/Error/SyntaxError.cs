using JMC.Extension.Server.Lexer.Error.Base;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Lexer.Error
{
    internal class SyntaxError(Position position, string expected, string current) : BaseError($"SyntaxError: at line:{position.Line} character: {position.Character}; expected {expected} but got {current}", ErrorType.IDE)
    {
    }
}
