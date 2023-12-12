using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        public string[] GetVariableNames()
        {
            var vars = from node in FlattenedNodes
                       where node.NodeType == Types.JMCSyntaxNodeType.Variable
                       select node.Value;
            var varCalls = from node in FlattenedNodes
                           where node.NodeType == Types.JMCSyntaxNodeType.VariableCall
                           select node.Value[..^6] ?? null;
            var q = vars.Concat(varCalls).Where(v => v != null).Distinct().ToArray();
            return q!;
        }

        public string[] GetFunctionNames()
        {
            var arr = FlattenedNodes.AsSpan();
            List<string> result = [];
            for (var i = 0; i < arr.Length; i++)
            {
                ref var current = ref arr[i];
                var previousValue = i - 1 != -1 ? arr[i - 1] : null;
                if (previousValue != null &&
                    previousValue.NodeType == JMCSyntaxNodeType.Function &&
                    current.NodeType == JMCSyntaxNodeType.Literal &&
                    current.Value != null)
                    result.Add(current.Value);

            }
            return [.. result];
        }
    }
}
