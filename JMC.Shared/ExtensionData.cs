using Brigadier.NET;
using JMC.Shared.Datas.BuiltIn;
using JMC.Shared.Datas.Minecraft.Command;
using Newtonsoft.Json;
using System.Reflection;

namespace JMC.Shared
{
    internal class ExtensionData
    {
        public static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static string MinecraftVersion = string.Empty;
#pragma warning disable CS8618
        public static CommandTree CommandTree { get; set; }
#pragma warning restore CS8618
        public static JMCBuiltInFunctionContainer JMCBuiltInFunctions { get; private set; } = 
            new JMCBuiltInFunctionContainer(GetJMCBuiltInFunctions());
        public static CommandDispatcher<object> CommandDispatcher { get; set; } = new();
        public ExtensionData() => UpdateVersion("1.20.1");

        //TODO
        public static void RegisterCommands()
        {
            var nodes = CommandTree.Nodes.ToArray().AsSpan();
            for (int i = 0; i < nodes.Length; i++)
            {
                ref var node = ref nodes[i];
            }
        }

        /// <summary>
        /// Update a minecraft version
        /// </summary>
        /// <param name="version"></param>
        public static void UpdateVersion(string version)
        {
            MinecraftVersion = version;
            CommandTree = new(GetCommandNodes(MinecraftVersion));
            RegisterCommands();
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static Dictionary<string, CommandNode> GetCommandNodes(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();
            var resouceStream = asm.GetManifestResourceStream($"JMC.Shared.Resource.{v}_commands.json") ?? throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<CommandNode>(jsonText) ?? throw new NotImplementedException(); ;
            return root.Children ?? throw new NotImplementedException(); ;
        }

        /// <summary>
        /// Get built in functions for JMC
        /// </summary>
        /// <returns></returns>
        private static JMCBuiltInFunction[] GetJMCBuiltInFunctions()
        {
            var asm = Assembly.GetExecutingAssembly();
            var resouceStream = asm.GetManifestResourceStream($"JMC.Shared.Resource.BuiltInFunctions.json") ?? throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();
            reader.Dispose();

            var data = JsonConvert.DeserializeObject<JMCBuiltInFunction[]>(jsonText) ?? throw new NotImplementedException();
            return data;
        }
    }
}
