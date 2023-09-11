using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Lexer.HJMC.Types
{
    public enum HJMCTokenType
    {
        UNKNOWN,
        DEFINE,
        BIND,
        CREDIT,
        INCLUDE,
        COMMAND,
        DEL,
        OVERRIDE,
        UNINSTALL,
        STATIC,
        NOMETA
    }
}
