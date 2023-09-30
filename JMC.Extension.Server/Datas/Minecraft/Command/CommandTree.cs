using JMC.Extension.Server.Datas.Minecraft.Command.Types;

namespace JMC.Extension.Server.Datas.Minecraft.Command
{
    //TODO: add enum for type, registry and parser .....
    internal class CommandTree(Dictionary<string, CommandNode> nodes)
    {
        public Dictionary<string, CommandNode> Nodes { get; } = nodes;
        public string[] RootCommands { get; } = [.. nodes.Keys];

        /// <summary>
        /// Get the nodes of the command string
        /// </summary>
        /// <param name="command"></param>
        /// <returns><see cref="CommandNode"/> of the command</returns>
        //TODO vec2 and vec3
        public IEnumerable<CommandNode> GetCommandNodes(IEnumerable<string> command)
        {
            var current = Nodes.First(v => v.Key == command.ElementAt(0)).Value;
            if (current == null) yield break;
            else yield return current;

            for (var i = 1; i < command.Count() - 1; i++)
            {
                var c = command.ElementAt(i);
                current = current.Query(c);
                if (current == null) yield break;
                else yield return current;
                if (current.Parser == CommandNodeParserType.VEC2) i++;
                else if (current.Parser == CommandNodeParserType.VEC3) i += 2;
            }
        }

    }
}