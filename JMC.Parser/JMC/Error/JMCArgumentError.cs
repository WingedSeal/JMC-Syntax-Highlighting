using JMC.Parser.JMC.Error.Base;
using JMC.Shared.Datas.BuiltIn;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC.Error
{
    internal class JMCArgumentError : JMCBaseError
    {
        public JMCArgumentError(Position position, JMCFunctionArgument arg) : 
            base($"ArgumentError: at line:{position.Line + 1} character: {position.Character + 1}; Missing argument '{arg.Name}'", ErrorType.IDE) { }
    }
}
