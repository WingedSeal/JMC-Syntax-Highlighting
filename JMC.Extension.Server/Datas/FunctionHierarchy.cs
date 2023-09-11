using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Helper;
using Serilog;

namespace JMC.Extension.Server.Datas
{
    internal sealed class FunctionHierarchy
    {
        public struct QueryData
        {
            public string FuncName { get; set; }
            public FunctionHierarchyType Type { get; set; }
            public QueryData(string funcName, FunctionHierarchyType type)
            {
                FuncName = funcName;
                Type = type;
            }
        }

        public static List<QueryData> GetFirstHierarchy(IEnumerable<string> funcs)
        {
            var list = new List<QueryData>();
            var arr = funcs.Select(v => v.Split(' ').Last().Split('.')).ToArray().AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var v = ref arr[i];
                if (v.Length == 0) continue;
                else if (v.Length == 1) list.Add(new(v[0], FunctionHierarchyType.FUNC));
                else list.Add(new(v[0], FunctionHierarchyType.CLASS));
            }
            return list.Distinct().ToList();
        }

        public static IEnumerable<QueryData> GetHierachy(IEnumerable<string> funcs, IEnumerable<string> preStrings)
        {
            var result = new List<QueryData>();

            var depth = preStrings.Count();
            var splited = funcs.Select(v => v.Split(' ').Last().Split('.'));
            var query = splited.Where(v => v.Length > depth).Where(v => v[0..depth].SequenceEqual(preStrings));

            var arr = query.ToArray().AsSpan();
            for (var i = 0; i < arr.Length; i++ )
            {
                ref var v = ref arr[i];

                if (v.Length > depth + 1) result.Add(new(v[depth], FunctionHierarchyType.CLASS));
                else result.Add(new(v[depth], FunctionHierarchyType.FUNC));
            }

            return result.Distinct();
        }
    }

    internal enum FunctionHierarchyType
    {
        CLASS, FUNC
    }
}
