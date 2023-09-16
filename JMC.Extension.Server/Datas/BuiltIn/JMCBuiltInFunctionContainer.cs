using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Extension.Server.Datas.BuiltIn
{
    internal class JMCBuiltInFunctionContainer : List<JMCBuiltInFunction>
    {
        public JMCBuiltInFunctionContainer(IEnumerable<JMCBuiltInFunction> list) : base(list) { }
        public IEnumerable<JMCBuiltInFunction> GetFunctions(string @class) => this.Where(v => v.Class == @class);
        public JMCBuiltInFunction? GetFunction(string @class, string func) => Find(v => v.Class == @class && v.Function == func);
    }
}
