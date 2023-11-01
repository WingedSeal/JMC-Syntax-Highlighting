using JMC.Shared;
using System.Text.Json;
using System.Text.RegularExpressions;
using static JMC.Shared.Datas.Minecraft.Command.CommandNode;

namespace JMC.Parser.JMC.Command
{
    internal partial class CommandParser
    {
        private static bool ExpectBool(string value) => value == "true" || value == "false";
        private static bool ExpectDouble(string value, Propety? propety = null)
        {
            var success = double.TryParse(value, out double d);
            if (propety != null)
            {
                if (!success) return false;
                else if (propety.Max == null || propety.Min == null) return true;
                else if (propety.Max != null && propety.Min != null) return propety.Max > d && d > propety.Min;
                else if (propety.Max != null) return propety.Max > d;
                //if (propety.Min != null) 
                else return propety.Min < d;
            }
            else return success;
        }

        private static bool ExpectFloat(string value, Propety? propety = null)
        {
            var success = float.TryParse(value, out float d);
            if (propety != null)
            {
                if (!success) return false;
                else if (propety.Max == null || propety.Min == null) return true;
                else if (propety.Max != null && propety.Min != null) return propety.Max > d && d > propety.Min;
                else if (propety.Max != null) return propety.Max > d;
                //if (propety.Min != null) 
                else return propety.Min < d;
            }
            else return success;
        }

        private static bool ExpectInt(string value, Propety? propety = null)
        {
            var success = int.TryParse(value, out int d);
            if (propety != null)
            {
                if (!success) return false;
                else if (propety.Max == null || propety.Min == null) return true;
                else if (propety.Max != null && propety.Min != null) return propety.Max > d && d > propety.Min;
                else if (propety.Max != null) return propety.Max > d;
                //if (propety.Min != null) 
                else return propety.Min < d;
            }
            else return success;
        }

        private static bool ExpectLong(string value, Propety? propety = null)
        {
            var success = long.TryParse(value, out long d);
            if (propety != null)
            {
                if (!success) return false;
                else if (propety.Max == null || propety.Min == null) return true;
                else if (propety.Max != null && propety.Min != null) return propety.Max > d && d > propety.Min;
                else if (propety.Max != null) return propety.Max > d;
                //if (propety.Min != null) 
                else return propety.Min < d;
            }
            else return success;
        }

        private bool ExpectVec2(string value)
        {
            var startIndex = Index;
            string[] pos = [value, Read()];

            var caret = pos.All(x => x.StartsWith('^'));
            var tilde = pos.All(x => x.StartsWith('~'));
            var num = pos.All(x => double.TryParse(x, out _));
            Index = startIndex;
            return caret || tilde || num;
        }

        private bool ExpectVec3(string value)
        {
            var startIndex = Index;
            string[] pos = [value, Read(), Read()];

            var caret = pos.All(x => x.StartsWith('^') && (x.Length == 1 || double.TryParse(x[1..], out _)));
            var tilde = pos.All(x => x.StartsWith('~') && (x.Length == 1 || double.TryParse(x[1..], out _)));
            var num = pos.All(x => double.TryParse(x, out _));
            Index = startIndex;
            return caret || tilde || num;
        }

        private static bool ExpectAngle(string value) => double.TryParse(value, out _) || value.StartsWith('~');

        private static bool ExpectString(string value, Propety propety) => propety.Type switch
        {
            "word" => NonSpaceRegex().IsMatch(value),
            "phrase" => PhraseRegex().IsMatch(value),
            _ => false
        };
        [GeneratedRegex(@"^\S+$", RegexOptions.Compiled)]
        private static partial Regex NonSpaceRegex();
        [GeneratedRegex(@"^([""\'].*[""\'])$")]
        private static partial Regex PhraseRegex();

        private static bool ExpectBlock(string value) => ExtensionData.BlockDatas.IsExist(value);
        private static string[] ChatColors => [
            "reset",
            "block",
            "dark_blue",
            "dark_green",
            "dark_aqua",
            "dark_red",
            "dark_purple",
            "gold",
            "gray",
            "dark_gray",
            "blue",
            "green",
            "aqua",
            "red",
            "light_purple",
            "yellow",
            "white",
        ];
        private static bool ExpectColor(string value) => ChatColors.Contains(value);
        private static bool ExpectComponent(string value)
        {
            try
            {
                JsonDocument.Parse(value);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
        private static bool ExpectResourceLocation(string value) => value.Count(v => v == ':') == 1;
        private static bool ExpectEntity(string value, Propety? propety = null)
        {
            var entityProp = GetEntityProperties(value);
            var isSingleProp = entityProp != null && entityProp.ContainsKey("limit") && entityProp["limit"] == "1";

            var amount = propety?.Amount;
            var type = propety?.Type;
            if ((amount == null && type == null) || amount == "multiple")
                return
                    ExpectUUID(value) ||
                    SelectorRegex().IsMatch(value) ||
                    PlayerRegex().IsMatch(value);
            else if (amount == "single" && type == null)
                return
                    ExpectUUID(value) ||
                    (SelectorRegex().IsMatch(value) && isSingleProp) ||
                    SingleSelectorRegex().IsMatch(value) ||
                    PlayerRegex().IsMatch(value);
            else if (amount == "single" && type == "players")
                return
                    ExpectUUID(value) ||
                    PlayerRegex().IsMatch(value) ||
                    SingleSelectorRegex().IsMatch(value) ||
                    (value.StartsWith("@a") && isSingleProp);
            else if (amount == "single" && type == "entities")
                return
                    ExpectUUID(value) ||
                    PlayerRegex().IsMatch(value) ||
                    SingleSelectorRegex().IsMatch(value) ||
                    (SelectorRegex().IsMatch(value) && isSingleProp);
            return false;
        }
        private static Dictionary<string, string>? GetEntityProperties(string value)
        {
            var dict = new Dictionary<string, string>();
            var arr = value.ToCharArray().AsSpan();
            for (var i = 0; i < value.Length; i++)
            {
                ref var current = ref arr[i];
                if (current == '[')
                {
                    i++;
                    var counter = 1;
                    var str = "";
                    str += current;
                    for (; i < value.Length; i++)
                    {
                        ref var @char = ref arr[i];
                        counter += @char switch
                        {
                            '[' => 1,
                            ']' => -1,
                            _ => 0
                        };
                        str += @char;
                        if (counter == 0) break;
                    }
                    if (!str.EndsWith(']')) return null;
                    str = str[1..str.Length];
                    var split = str.Split(',').AsSpan();
                    for (var j = 0; j < split.Length; j++)
                    {
                        ref var newStr = ref split[j];
                        var splitStr = newStr.Split("=");
                        var head = splitStr.First();
                        var tail = splitStr.Last();
                        dict.Add(head, tail);
                    }
                }
            }
            return dict;
        }
        private static bool ExpectUUID(string value) => UUIDRegex().IsMatch(value);
        [GeneratedRegex(@"^(?:[a-zA-Z0-9]){8}-(?:[a-zA-Z0-9]){4}-(?:[a-zA-Z0-9]){4}-(?:[a-zA-Z0-9]){4}-(?:[a-zA-Z0-9]){12}$", RegexOptions.Compiled)]
        private static partial Regex UUIDRegex();
        [GeneratedRegex(@"^@[parse]", RegexOptions.Compiled)]
        private static partial Regex SelectorRegex();
        [GeneratedRegex(@"^@[prs]", RegexOptions.Compiled)]
        private static partial Regex SingleSelectorRegex();
        [GeneratedRegex(@"^[0-9a-zA-Z_-]$", RegexOptions.Compiled)]
        private static partial Regex PlayerRegex();

        private static bool ExpectEntityAnchor(string value) => value == "eyes" || value == "feet";

        private static bool ExpectFloatRange(string value)
        {
            var nums = value.Split("..");
            var head = nums.First();
            var tail = nums.Last();

            return 
                float.TryParse(head, out float h) && 
                float.TryParse(tail, out float t) && 
                h >= 0.1 && t <= 1;
        }
        
        private static bool ExpectFunction(string value) => FunctionCallRegex().IsMatch(value);
        [GeneratedRegex(@"^(\S+)+\([^\(]*\)$", RegexOptions.Compiled)]
        private static partial Regex FunctionCallRegex();
    }
}
