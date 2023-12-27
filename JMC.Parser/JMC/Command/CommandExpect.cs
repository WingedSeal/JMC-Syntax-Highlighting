using JMC.Shared;
using System.Collections.Immutable;
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
        [GeneratedRegex(@"^(?:([^().]+)\s*(?:\.\s*)?)+\(([^\)]*)\)$", RegexOptions.Compiled)]
        private static partial Regex FunctionCallRegex();
        private static bool ExpectGameProfile(string value) => ExpectEntity(value, new()
        {
            Type = "players",
            Amount = "multiple"
        });
        private static readonly ImmutableArray<string> Gamemodes = [
            "survival",
            "creative",
            "adventure",
            "spectator"
        ];
        private static bool ExpectGamemode(string value) => Gamemodes.Contains(value);
        private static readonly ImmutableArray<string> Heightmaps = [
            "world_surface",
            "motion_blocking",
            "motion_blocking_no_leaves",
            "ocean_floor"
        ];
        private static bool ExpectHeightmap(string value) => Heightmaps.Contains(value);
        private static bool ExpectIntRange(string value)
        {
            var split = value.Split("..");
            var head = split.First();
            var tail = split.Last();

            var h = int.TryParse(head, out _);
            var t = int.TryParse(tail, out _);

            return h && t;
        }
        private static bool ExpectItemPredicate(string value) => ExtensionData.ItemDatas.IsExist(value);
        private bool ExpectItemSlot(string value)
        {
            if (int.TryParse(value, out int v))
            {
                if (!(v == -106 || v <= 514))
                    Errors.Add(StartReadIndex, $"Invalid slot id {v}");

                return true;
            }
            var split = value.Split('.');
            var slot = split.First();
            var id = split.Last();
            var isIdNum = int.TryParse(id, out int num);

            switch (slot)
            {
                case "weapon":
                    if (split.Length == 1 || id == "mainhand" || id == "offhand")
                        return true;
                    break;
                case "container":
                    if (split.Length > 1 && isIdNum && num >= 0 && num <= 53)
                        return true;
                    break;
                case "inventory":
                case "enderchest":
                    if (split.Length > 1 && isIdNum && num >= 0 && num <= 26)
                        return true;
                    break;
                case "hotbar":
                    if (split.Length > 1 && isIdNum && num >= 0 && num <= 53)
                        return true;
                    break;
                case "horse":
                    if (split.Length > 1 && isIdNum && num >= 0 && num <= 14)
                        return true;
                    else if (id == "saddle" || id == "chest" || id == "armor")
                        return true;
                    break;
                case "villager":
                    if (split.Length > 1 && isIdNum && num >= 0 && num <= 7)
                        return true;
                    break;
                case "armor":
                    if (id == "chest" || id == "feet" || id == "head" || id == "legs")
                        return true;
                    break;
                default:
                    break;
            }
            Errors.Add(StartReadIndex, $"Invalid slot id {value}");
            return false;
        }
        private static bool ExpectNBT(string value) => value.StartsWith('{') && value.EndsWith('}');
        private static bool ExpectObjective(string value) => NonSpaceRegex().IsMatch(value);
        private static bool ExpectObjectiveCriteria(string value)
        {
            var split = value.Split(':');
            var head = split.First();
            if (split.Length > 1 && head != "minecraft")
                return false;
            else if (split.Length > 1)
                return ExtensionData.ScoreboardCriterions.Contains(value);
            return ExtensionData.ScoreboardCriterions.Contains($"minecraft:{value}");
        }
        private static readonly ImmutableArray<string> Operators = [
            "=",
            "+=",
            "-=",
            "/=",
            "*=",
            "%=",
            "><",
            "<",
            ">"
        ];
        private static bool ExpectOperation(string value) => Operators.Contains(value);
        private bool ExpectParticle(string value)
        {
            var split = value.Split(':');
            var head = split.First();
            if (split.Length > 1 && head != "minecraft")
                return false;
            value = split.Length == 1 ? $"minecraft:{value}" : value;

            if (value == "minecraft:block" || value == "minecraft:block_marker" || value == "minecraft:falling_dust")
                return ExpectBlock(Read());
            else if (value == "item")
                return ExpectItemPredicate(Read());
            else if (value == "minecraft:dust")
                return new bool[]
                {
                    ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read())
                }.All(v => v);
            else if (value == "minecraft:dust_color_transition")
                return new bool[]
                {
                    ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read())
                }.All(v => v);
            else if (value == "minecraft:shriek")
                return ExpectInt(Read());
            else if (value == "minecraft:vibration")
                return new bool[]
                {
                    ExpectFloat(Read()),ExpectFloat(Read()),ExpectFloat(Read()),ExpectInt(Read())
                }.All(v => v);
            else return ExtensionData.Particles.Contains(value);
        }
        private static bool ExpectResource(string value) => NonSpaceRegex().IsMatch(value);
        private static readonly ImmutableArray<string> TeamColors = [
            "black",
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
            "white"
        ];
        private static bool ExpectScoreboardSlot(string value)
        {
            if (value.StartsWith("sidebar.team."))
                return TeamColors.Contains(value.Split('.').Last());
            return value == "list" || value == "sidebar" || value == "belowName";
        }

        private static bool ExpectSwizzle(string value)
        {
            var xCount = value.Count(v => v == 'x') <= 1;
            var yCount = value.Count(v => v == 'y') <= 1;
            var zCount = value.Count(v => v == 'z') <= 1;
            return xCount && yCount && zCount;
        }

        private static bool ExpectTeam(string value) => TeamRegex().IsMatch(value);
        [GeneratedRegex(@"^[-+\._a-zA-Z0-9]+$", RegexOptions.Compiled)]
        private static partial Regex TeamRegex();
        private static bool ExpectTemplateMirror(string value) => value == "none" || value == "front_back" || value == "left_right";
        private static bool ExpectTemplateRotation(string value) => value == "none" || value == "clockwise_90" || value == "counterclockwise_90" || value == "180";
        private static bool ExpectTime(string value, Propety? propety = null)
        {
            var isLastNum = int.TryParse(value.Last().ToString(), out _);
            var lastChar = value.Last();
            float num;
            bool success;
            if (isLastNum)
                success = float.TryParse(value[..1], out num);
            else
                success = float.TryParse(value, out num);
            if (success && propety != null && propety.Min != null)
            {
                var ticks = lastChar switch
                {
                    'd' => num * 24000,
                    's' => num * 20,
                    _ => num
                };
                return ticks > propety.Min;
            }
            return success;
        }
    }
}
