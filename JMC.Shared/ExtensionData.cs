using JMC.Shared.Datas.BuiltIn;
using JMC.Shared.Datas.Minecraft;
using JMC.Shared.Datas.Minecraft.Command;
using System.Text.Json;

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
        public static JMCBuiltInFunctionContainer JMCBuiltInFunctions { get; private set; }
#pragma warning restore CS8618
        private static string ModifiedVersion = string.Empty;
        private static readonly JMCDatabase Database = new();
        public ExtensionData()
        {
            Database.DatabaseConnection.Open();
            JMCBuiltInFunctions = new(GetJMCBuiltInFunctions());
            UpdateVersion("1.20.1");
            Database.DatabaseConnection.Close();
        }

        /// <summary>
        /// Update a minecraft version
        /// </summary>
        /// <param name="version"></param>
        public static void UpdateVersion(string version)
        {
            MinecraftVersion = version;
            ModifiedVersion = version.Replace(".", "_") ?? throw new NotImplementedException();
            Database.Version = ModifiedVersion;

            CommandTree = new(GetCommandNodes());
            ScoreboardCriterions = new(GetCustomStatistics().Concat(["minecraft:dummy", "minecraft:trigger", "minecraft:deathCount", "minecraft:playerKillCount", "minecraft:totalKillCount", "minecraft:health", "minecraft:xp", "minecraft:level", "minecraft:food", "minecraft:air", "minecraft:armor"]));
        }

        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> GetCustomStatistics()
        {
            var jsonText = Database.GetMinecraftFileString("custom_statistics");

            var root = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonText) ?? throw new NotImplementedException();
            var values = root.Keys.ToList();
            return values ?? throw new NotImplementedException(); ;
        }



        /// <summary>
        /// Json command tree to memory data
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static Dictionary<string, CommandNode> GetCommandNodes()
        {
            var jsonText = Database.GetMinecraftFileString("commands");

            var root = JsonSerializer.Deserialize<CommandNode>(jsonText) ?? throw new NotImplementedException();
            return root.Children ?? throw new NotImplementedException();
        }

        /// <summary>
        /// Get built in functions for JMC
        /// </summary>
        /// <returns></returns>
        private static JMCBuiltInFunction[] GetJMCBuiltInFunctions()
        {
            var jsonText = Database.GetBuiltinFunctionString();

            var data = JsonSerializer.Deserialize<JMCBuiltInFunction[]>(jsonText) ?? throw new NotImplementedException();
            return data;
        }
    }
}
