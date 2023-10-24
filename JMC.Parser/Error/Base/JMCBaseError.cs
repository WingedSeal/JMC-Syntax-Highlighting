using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Parser.Error.Base
{
    internal abstract class JMCBaseError(string message, ErrorType errorType)
    {
        public string Message { get; private set; } = message;
        public ErrorType ErrorType { get; private set; } = errorType;
    }
}
