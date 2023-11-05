using JMC.Shared.Datas.BuiltIn;
using JMC.Shared.Datas.Minecraft.Blocks;
using JMC.Shared.Datas.Minecraft.Command;
using JMC.Shared.Datas.Minecraft.Items;
using Newtonsoft.Json;
using System.Reflection;

namespace JMC.Shared
{
    internal class ExtensionData
    {
        public static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static string MinecraftVersion = string.Empty;
#pragma warning disable CS8618
        public static CommandTree CommandTree { get; private set; }
        public static BlockDataContainer BlockDatas { get; private set; }
        public static ItemDataContainer ItemDatas { get; private set; }
        public static List<string> BlockTags { get; private set; }
        public static List<string> ItemTags { get; private set; }
        public static List<string> ScoreboardCriterions { get; private set; }
        public static List<string> Particles { get; private set; }
#pragma warning restore CS8618
        public static JMCBuiltInFunctionContainer JMCBuiltInFunctions { get; private set; } =
            new JMCBuiltInFunctionContainer(GetJMCBuiltInFunctions());
        public ExtensionData() => UpdateVersion("1.20.1");

        /// <summary>
        /// Update a minecraft version
        /// </summary>
        /// <param name="version"></param>
        public static void UpdateVersion(string version)
        {
            MinecraftVersion = version;
            CommandTree = new(GetCommandNodes(MinecraftVersion));
            BlockDatas = new(GetBlockDatas(MinecraftVersion));
            ItemDatas = new(GetItemDatas(MinecraftVersion));
            BlockTags = new(GetBlockTags(MinecraftVersion));
            ItemTags = new(GetItemTags(MinecraftVersion));
            ScoreboardCriterions = new(GetCustomStatistics(MinecraftVersion));
            Particles = new(GetParticles(MinecraftVersion));
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetParticles(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.particles.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText) ?? throw new NotImplementedException();
            return root.Keys ?? throw new NotImplementedException(); ;
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetCustomStatistics(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.custom_statistics.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText) ?? throw new NotImplementedException();
            var values = root.Keys.ToList();
            string[] str = ["dummy", "trigger", "deathCount", "playerKillCount", "totalKillCount", "health", "xp", "level", "food", "air", "armor"];
            str = str.Select(v => $"minecraft:{v}").ToArray();
            values.AddRange(str);
            return values ?? throw new NotImplementedException(); ;
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetItemTags(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.item_tags.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText) ?? throw new NotImplementedException(); ;
            return root.Keys ?? throw new NotImplementedException(); ;
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetBlockTags(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.block_tags.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText) ?? throw new NotImplementedException(); ;
            return root.Keys ?? throw new NotImplementedException(); ;
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

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.commands.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var root = JsonConvert.DeserializeObject<CommandNode>(jsonText) ?? throw new NotImplementedException(); ;
            return root.Children ?? throw new NotImplementedException(); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static List<BlockData> GetBlockDatas(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.blocks.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var data = JsonConvert.DeserializeObject<List<BlockData>>(jsonText) ?? throw new NotImplementedException(); ;
            return data ?? throw new NotImplementedException(); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static Dictionary<string, ItemData> GetItemDatas(string version)
        {
            var v = version.Replace(".", "_") ?? throw new NotImplementedException();
            var asm = Assembly.GetExecutingAssembly();

            //commands
            var resouceStream =
                asm.GetManifestResourceStream($"JMC.Shared.Resource._{v}.items.json") ??
                throw new NotImplementedException();
            var reader = new StreamReader(resouceStream);
            var jsonText = reader.ReadToEnd();

            reader.Dispose();

            var data = JsonConvert.DeserializeObject<Dictionary<string, ItemData>>(jsonText) ?? throw new NotImplementedException(); ;
            return data ?? throw new NotImplementedException(); ;
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
