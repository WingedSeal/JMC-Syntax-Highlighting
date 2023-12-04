using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        //TODO:
        public Dictionary<string, Range> GetVariablesRange()
        {
            var dict = new Dictionary<string, Range>();

            var variableNodes = FlattenedNodes.Where(v => v.NodeType == JMCSyntaxNodeType.Variable);
            var variableCallNodes = FlattenedNodes.Where(v => v.NodeType == JMCSyntaxNodeType.VariableCall);

            //add variables
            var arr = variableNodes.ToArray().AsSpan();
            for (var i = 0; i < variableNodes.Count(); i++)
            {
                ref var node = ref arr[i];
                if (node.Value != null && node.Range != null)
                    dict.Add(node.Value, node.Range);
            }

            //add variable calls
            arr = variableCallNodes.ToArray().AsSpan();
            for (var i = 0; i < variableNodes.Count(); i++)
            {
                ref var node = ref arr[i];
                if (node.Value != null && node.Range != null)
                    dict.Add(node.Value, node.Range);
            }

            return dict;
        }
    }
}
