namespace JMC.Shared.Datas.Minecraft.Command
{
    internal class CommandTree(Dictionary<string, CommandNode> nodes)
    {
        public Dictionary<string, CommandNode> Nodes { get; } = nodes;
        public string[] RootCommands { get; } = [.. nodes.Keys];
    }
}