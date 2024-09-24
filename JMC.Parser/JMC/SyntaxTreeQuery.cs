using JMC.Parser.JMC.Types;

namespace JMC.Parser.JMC
{
    internal partial class SyntaxTree
    {
        public string[] GetVariableNames()
        {
            var varaibles = from node in FlattenedNodes
                       where node.NodeType == SyntaxNodeType.Variable
                       select node.Value;
            var variableCalls = from node in FlattenedNodes
                           where node.NodeType == SyntaxNodeType.VariableCall
                           select node?.Value?[..^6] ?? null;
            var result = varaibles.Concat(variableCalls).Where(v => v != null).Distinct().ToArray();
            return result!;
        }

        public string[] GetFunctionNames()
        {
            return FlattenedNodes
                .Where(v => v.NodeType == SyntaxNodeType.Function && v.Value != null && v.Value.Split('.').Length == 1)
                .Select(v => v.Value!)
                .ToArray();
        }

        public string[] GetClassNames()
        {
            //get 'class' from class.func()
            var funcClass = FlattenedNodes
                .Where(v => v.NodeType == SyntaxNodeType.Function && v.Value != null && v.Value.Split('.').Length > 1)
                .Select(v => v.Value!.Split('.').First());
            //get normal class defined in class
            var normalClass = FlattenedNodes
                .Where(v => v.NodeType == SyntaxNodeType.Class && v.Value != null)
                .Select(v => v.Value!.Split('.').First());
            return normalClass.Concat(funcClass).ToArray();
        }

        public string GetConnectedClassName(int pos)
        {
            var node = FlattenedNodes
        .FirstOrDefault(v => v.Value != null && v.Value.Contains('.') && pos > v.Value.IndexOf('.') && pos <= v.Value.Length);

            if (node != null)
            {
                var parts = node.Value.Split('.');
                return parts[0]; // Return the word before the dot
            }

            return null; // Return null if no match is found
        }

    }
}
