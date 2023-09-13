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
        //TODO
        public JMCBuiltInFunctionContainer(IEnumerable<JMCBuiltInFunction> list) : base(list) { }
    }
}
