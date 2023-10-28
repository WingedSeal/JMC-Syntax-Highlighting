using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static JMC.Shared.Datas.Minecraft.Command.CommandNode;

namespace JMC.Parser.Helper
{
    internal static class CommandHelper
    {
        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;
        private static readonly ImmutableDictionary<string, Regex> CommandTypeTests = new Dictionary<string, Regex>()
        {
            ["brigadier:bool"] = new Regex(@"^true|false$", _regexOptions),
            ["brigadier:float"] = new Regex(@"^(-?\d*\.?\d+)$", _regexOptions),
            ["brigadier:double"] = new Regex(@"^(-?\d*\.?\d+)$", _regexOptions),
            ["brigadier:integer"] = new Regex(@"^(\d+)$", _regexOptions),
            ["brigadier:long"] = new Regex(@"^(\d+)$", _regexOptions),
            ["brigadier:string_word"] = new Regex(@"^\S+$", _regexOptions),
            ["brigadier:string_phrase"] = new Regex(@"^(\S+)|([""\'].*[""\'])$", _regexOptions),
            ["brigadier:string_greedy"] = new Regex(@"^.+$", _regexOptions),
            ["minecraft:angle"] = new Regex(@"^(~?-?\d*\.?\d+)$", _regexOptions),
        }.ToImmutableDictionary();
        public static readonly ImmutableArray<string> VEC2Types = [
            "minecraft:column_pos",
            "minecraft:rotation"
        ];
        public static readonly ImmutableArray<string> VEC3Types = [
            "minecraft:block_pos",
            "minecraft:vec3"
        ];
        /// <summary>
        /// not for VEC2 and VEC3
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propety"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsMatch(string type, Propety? propety, string value)
        {
            if (type == "brigadier:string" && propety != null && propety.Type != null)
            {
                type += $"_{propety.Type}";
            }
            var regex = CommandTypeTests[type] ?? null;

            var isMatch = false;
            if (regex != null)
                isMatch = regex.IsMatch(value);
            //TODO
            else
            {
                switch (type)
                {
                    case "minecraft:block_state":
                    default:
                        break;
                }
            }

            return isMatch;
        }
    }
}
