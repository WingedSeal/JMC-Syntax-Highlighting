using JMC.Parser.JMC.Error.Base;
using JMC.Shared.Datas.BuiltIn;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC.Error
{
    internal class JMCArgumentError(Range range, JMCFunctionArgument arg) : JMCBaseError($"ArgumentError: at line:{range.Start.Line + 1} character: {range.Start.Character + 1}; Missing argument '{arg.Name}'", ErrorType.IDE, range)
    {
    }
}
