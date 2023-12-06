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
    }
}
