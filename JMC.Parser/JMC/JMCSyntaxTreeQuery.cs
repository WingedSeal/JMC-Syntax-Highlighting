using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        public string[] GetVariableNames()
        {
            var vars = from node in FlattenedNodes
                       where node.NodeType == JMCSyntaxNodeType.Variable
                       select node.Value;
            var varCalls = from node in FlattenedNodes
                           where node.NodeType == JMCSyntaxNodeType.VariableCall
                           select node.Value[..^6] ?? null;
            var q = vars.Concat(varCalls).Where(v => v != null).Distinct().ToArray();
            return q!;
        }

        public string[] GetFunctionNames()
        {
            return FlattenedNodes
                .Where(v => v.NodeType == JMCSyntaxNodeType.Function && v.Value != null && v.Value.Split('.').Length == 1)
                .Select(v => v.Value!)
                .ToArray();
        }

        public string[] GetClassNames()
        {
            var funcClass = FlattenedNodes
                .Where(v => v.NodeType == JMCSyntaxNodeType.Function && v.Value != null && v.Value.Split('.').Length > 1)
                .Select(v => v.Value!.Split('.').First());
            var @class = FlattenedNodes
                .Where(v => v.NodeType == JMCSyntaxNodeType.Class && v.Value != null)
                .Select(v => v.Value!.Split('.').First());
            return @class.Concat(funcClass).ToArray();
        }
    }
}
