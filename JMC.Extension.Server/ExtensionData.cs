using JMC.Extension.Server.Datas.BuiltIn;
using JMC.Extension.Server.Datas.Minecraft.Command;
using JMC.Extension.Server.Datas.Workspace;
using JMC.Extension.Server.Lexer.Error.Base;
using Newtonsoft.Json;
using System.Reflection;

namespace JMC.Extension.Server
{
    internal class ExtensionData
    {
        public static readonly WorkspaceContainer Workspaces = new();
        public static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static string MinecraftVersion = string.Empty;
#pragma warning disable CS8618
        public static CommandTree CommandTree { get; set; }

        public static JMCBuiltInFunctionContainer JMCBuiltInFunctions { get; private set; }
#pragma warning restore CS8618
        public ExtensionData()
        {
            MinecraftVersion = "1.20.1";
            CommandTree = new(GetCommandNodes(MinecraftVersion));
            JMCBuiltInFunctions = new JMCBuiltInFunctionContainer(GetJMCBuiltInFunctions());
        }

        /// <summary>
        /// Update a minecraft version
        /// </summary>
        /// <param name="version"></param>
        public static void UpdateVersion(string version)
        {
            MinecraftVersion = version;
            CommandTree = new(GetCommandNodes(MinecraftVersion));
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static Dictionary<string, CommandNode> GetCommandNodes(string version)
        {
            var v = version.Replace(".", "_");
            var asm = Assembly.GetExecutingAssembly();
            var resouceStream = asm.GetManifestResourceStream($"JMC.Extension.Server.Resource.{v}_commands.json");
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<CommandNode>(jsonText);
            return root.Children;
        }

        /// <summary>
        /// Get built in functions for JMC
        /// </summary>
        /// <returns></returns>
        private static JMCBuiltInFunction[] GetJMCBuiltInFunctions()
        {
            var asm = Assembly.GetExecutingAssembly();
            var resouceStream = asm.GetManifestResourceStream($"JMC.Extension.Server.Resource.BuiltInFunctions.json");
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();
            reader.Dispose();

            var data = JsonConvert.DeserializeObject<JMCBuiltInFunction[]>(jsonText);
            return data;
        }
    }
}
