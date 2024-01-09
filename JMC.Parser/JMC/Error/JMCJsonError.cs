using JMC.Parser.JMC.Error.Base;

namespace JMC.Parser.JMC.Error
{
    internal class JMCJsonError(Range range, string msg) : JMCBaseError($"at {range}:{msg}", ErrorType.IDE, range)
    {
    }
}
