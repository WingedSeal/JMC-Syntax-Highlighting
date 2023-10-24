using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JMC.Shared.Datas
{
    internal struct JMCConfig
    {
        public string Namespace { get; set; }
        public string Description { get; set; }
        public int PackFormat { get; set; }
        public string Target { get; set; }
        public string Output { get; set; }
    }
}
