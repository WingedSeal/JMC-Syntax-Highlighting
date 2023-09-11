using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JMC.Extension.Server.Datas.Minecraft.Command.Types;

namespace JMC.Extension.Server.Datas.Minecraft.Command
{
    //TODO: add enum for type, registry and parser .....
    internal class CommandData
    {
        public Dictionary<string, CommandNode> Nodes { get; }
        public string[] RootCommands { get; }

        public CommandData(Dictionary<string, CommandNode> nodes)
        {
            Nodes = nodes;
            RootCommands = nodes.Keys.ToArray();
        }

        /// <summary>
        /// Get the nodes of the command string, seperated by whitespace characters
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public IEnumerable<CommandNode> GetCommandNodes(IEnumerable<string> command)
        {
            var current = Nodes.First(v => v.Key == command.ElementAt(0)).Value;
            if (current == null) yield break;
            else yield return current;

            foreach (var c in command)
            {
                current = Query(current, c);
                if (current == null) yield break;
                else yield return current;
            }
        }

        /// <summary>
        /// Query for a string in the node's childrens
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static CommandNode? Query(CommandNode currentNode ,string query)
        {
            var childrens = currentNode.Childrens;
            if (childrens == null) 
                return null;
            else if (childrens.Count == 1 && 
                childrens.ElementAt(0).Value.Type == CommandNodeType.ARGUMENT) 
                return childrens.ElementAt(0).Value;

            return currentNode.Childrens?.FirstOrDefault(v => v.Key == query).Value;
        }
    }
}
