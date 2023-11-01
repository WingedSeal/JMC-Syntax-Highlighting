using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace JMC.Shared.Datas.Minecraft.Command
{
    internal partial class CommandNode
    {
        [JsonProperty("type")]
        [JsonRequired]
#pragma warning disable CS8618
        public string Type { get; set; }
#pragma warning restore CS8618
        [JsonProperty("children")]
        public Dictionary<string, CommandNode>? Children { get; set; }
        [JsonProperty("executable")]
        public bool? Executable { get; set; }
        [JsonProperty("parser")]
        public string? Parser { get; set; }

        [JsonProperty("properties")]
        public Propety? Properties { get; set; }
        [JsonProperty("redirect")]
        public string[]? Redirect {  get; set; }

        public class Propety
        {
            [JsonProperty("type")]
            public string? Type { get; set; }
            [JsonProperty("registry")]
            public string? Registry { get; set; }
            [JsonProperty("amount")]
            public string? Amount { get; set; }
            [JsonProperty("min")]
            public double? Min { get; set; }
            [JsonProperty("max")]
            public double? Max { get; set; }
        }

        private Regex NumberPattern = NumberRegex();
        private Regex SelectorPattern = SelectorRegex();


        [GeneratedRegex(@"^-?\d*\.?\d+[lbs]?$")]
        private static partial Regex NumberRegex();
        [GeneratedRegex(@"^@[parse]$")]
        private static partial Regex SelectorRegex();
    }
}
