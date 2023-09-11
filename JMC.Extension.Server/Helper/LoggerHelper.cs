using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Helper
{
    internal static class LoggerHelper
    {
        public static string ObjectToJson(object obj) => JsonSerializer.Serialize(obj);
    }
}
