using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace JMC.Shared.Datas.Minecraft.Command
{
    internal partial class CommandNode
    {
        [JsonPropertyName("type")]
        [JsonRequired]
#pragma warning disable CS8618
        public string Type { get; set; }
#pragma warning restore CS8618
        [JsonPropertyName("children")]
        public Dictionary<string, CommandNode>? Children { get; set; }
        [JsonPropertyName("executable")]
        public bool? Executable { get; set; }
        [JsonPropertyName("parser")]
        public string? Parser { get; set; }

        [JsonPropertyName("properties")]
        public Propety? Properties { get; set; }
        [JsonPropertyName("redirect")]
        public string[]? Redirect { get; set; }

        public class Propety
        {
            [JsonPropertyName("type")]
            public string? Type { get; set; }
            [JsonPropertyName("registry")]
            public string? Registry { get; set; }
            [JsonPropertyName("amount")]
            public string? Amount { get; set; }
            [JsonPropertyName("min")]
            public double? Min { get; set; }
            [JsonPropertyName("max")]
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
