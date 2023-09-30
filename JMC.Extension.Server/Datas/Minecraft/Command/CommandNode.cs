using JMC.Extension.Server.Datas.Minecraft.Command.Types;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace JMC.Extension.Server.Datas.Minecraft.Command
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

        [JsonProperty("propeties")]
        public List<Propety>? Propeties { get; set; }

        public class Propety
        {
            [JsonProperty("type")]
            public string? Type { get; set; }
            [JsonProperty("registry")]
            public string? Registry { get; set; }
            [JsonProperty("amount")]
            public string? Amount { get; set; }
            [JsonProperty("min")]
            public int? Min { get; set; }
            [JsonProperty("max")]
            public int? Max { get; set; }
        }

        private Regex NumberPattern = NumberRegex();
        private Regex SelectorPattern = SelectorRegex();

        /// <summary>
        /// Query for a string in the node's childrens
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public CommandNode? Query(string query)
        {
            if (Children == null)
                return null;
            //else if (Children.Count == 1 &&
            //    Children.ElementAt(0).Value.Type == CommandNodeType.ARGUMENT)
            //    return Children.ElementAt(0).Value;
            //return Children?.FirstOrDefault(v => v.Key == query).Value;

            var arr = Children.Values.ToArray().AsSpan();

            for (var i = 0; i < arr.Length; i++)
            {
                var child = arr[i];
                if (child.Type == CommandNodeType.LITERAL && Children.ContainsKey(query)) return child;
                else if (child.Type == CommandNodeType.ARGUMENT)
                {
                    var parser = child.Parser;
                    switch (parser)
                    {
                        case CommandNodeParserType.VEC2:
                        case CommandNodeParserType.VEC3:
                            if (NumberPattern.IsMatch(query)) return child;
                            break;
                        case CommandNodeParserType.ENTITY:
                            if (SelectorPattern.IsMatch(query)) return child;
                            break;
                            //TODO add parsers
                        default:
                            break;
                    }
                }
            }
            return null;
        }

        [GeneratedRegex(@"^-?\d*\.?\d+[lbs]?$")]
        private static partial Regex NumberRegex();
        [GeneratedRegex(@"^@[parse]$")]
        private static partial Regex SelectorRegex();
    }
}
