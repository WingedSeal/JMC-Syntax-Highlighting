using JMC.Parser.JMC.Error.Base;

namespace JMC.Parser.JMC.Error
{
    internal class JMCUnexpectedError(Exception exception) : JMCBaseError(exception.Message, ErrorType.Parser, new Range(0, 0, 0, 0))
    {
    }
}
