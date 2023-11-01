using JMC.Shared;
using JMC.Shared.Datas.Minecraft.Command;
using JMC.Shared.Datas.Minecraft.Command.Types;
using System.Collections.Immutable;

namespace JMC.Parser.JMC.Command
{
    //TODO Arguments types of commands
    //TODO Redirect of command
    internal partial class CommandParser(string cmdString, int offset, string text)
    {
        private string CommandString => cmdString;
        private int StartOffset => offset;
        private string TreeText => text;
        private int StartReadIndex { get; set; } = -1;
        private int Index { get; set; } = 0;
        private bool End => CurrentChar == '\0';
        private bool Started { get; set; } = false;
        private char CurrentChar => Index >= CommandString.Length ? '\0' : CommandString[Index];
        private CommandTree CommandTree { get; set; } = ExtensionData.CommandTree;
        private CommandNode? CurrentNode { get; set; } = null;

        //accessible props
        public Dictionary<int, string> Errors { get; private set; } = [];
        public List<JMCSyntaxNode> Nodes { get; private set; } = [];

        //Types
        private static ImmutableArray<string> Vec2Types => [
            "minecraft:vec2",
            "minecraft:column_pos"
        ];
        private static ImmutableArray<string> Vec3Types => [
            "minecraft:vec3",
            "minecraft:block_pos"
        ];
        public IEnumerable<JMCSyntaxNode> ParseCommand()
        {
            while (CurrentChar != '\0')
            {
                var elem = CurrentNode?.Children?.ElementAtOrDefault(0).Value;
                if (elem != null &&
                    elem.Parser == "brigadier:string" &&
                    elem.Properties?.Type == "greedy")
                {
                    CurrentNode = elem;
                    var s = ReadToEnd();
                    var n = GetSyntaxNode(s);
                    Nodes.Add(n);
                    break;
                }

                var r = Read();
                CurrentNode = GetCommandNode(r);

                if (CurrentNode == null) break;


                var node = GetSyntaxNode(r);
                Nodes.Add(node);

                if (CurrentNode?.Redirect != null)
                {
                    var q = CurrentNode.Redirect.First();
                    Started = false;
                    CurrentNode = GetCommandNode(q);
                }
            }

            //detect end
            if (CurrentNode == null && CurrentChar != '\0')
            {
                Errors.Add(Index, "Expect End");
            }
            else if (CurrentNode?.Executable == null)
            {
                var keys = CurrentNode?.Children?.Keys;
                var s = keys != null ? string.Join(" or ", keys.Select(v => $"'{v}'")) : "";
                Errors.Add(Index, $"Expect {s}");
            }
            return Nodes;
        }

        private JMCSyntaxNode GetSyntaxNode(string value)
        {
            if (CurrentNode == null) return new();

            if (CurrentNode.Parser != null && Vec2Types.Contains(CurrentNode.Parser))
            {
                string[] pos = [value, Read()];
                value = string.Join(' ', pos);
            }
            else if (CurrentNode.Parser != null && Vec3Types.Contains(CurrentNode.Parser))
            {
                string[] pos = [value, Read(), Read()];
                value = string.Join(' ', pos);
            }

            var node = new JMCSyntaxNode();
            var start = (StartOffset + StartReadIndex).ToPosition(TreeText);
            var end = (StartOffset + Index - 2).ToPosition(TreeText);
            var range = new Range(start, end);

            var parser = CurrentNode.Parser;
            var type = CurrentNode.Type == CommandNodeType.LITERAL ?
                JMCSyntaxNodeType.LITERAL :
                ToSyntaxNodeType(parser ?? "");

            node.Value = value;
            node.Range = range;
            node.NodeType = type;

            return node;
        }

        private JMCSyntaxNode GetSyntaxNode(string value, JMCSyntaxNodeType type)
        {
            if (CurrentNode == null) return new();

            var node = new JMCSyntaxNode();
            var start = (StartOffset + StartReadIndex).ToPosition(TreeText);
            var end = (StartOffset + Index - 2).ToPosition(TreeText);
            var range = new Range(start, end);

            node.Value = value;
            node.Range = range;
            node.NodeType = type;

            return node;
        }

        private static JMCSyntaxNodeType ToSyntaxNodeType(string parser) => parser switch
        {
            "brigadier:bool" => JMCSyntaxNodeType.BOOL,
            "brigadier:double" => JMCSyntaxNodeType.DOUBLE,
            "brigadier:float" => JMCSyntaxNodeType.FLOAT,
            "brigadier:integer" => JMCSyntaxNodeType.INT,
            "brigadier:long" => JMCSyntaxNodeType.LONG,
            "brigadier:string" => JMCSyntaxNodeType.COMMAND_STRING,
            "minecraft:block_pos" => JMCSyntaxNodeType.VEC3,
            "minecraft:vec3" => JMCSyntaxNodeType.VEC3,
            "minecraft:vec2" => JMCSyntaxNodeType.VEC2,
            "minecraft:angle" => JMCSyntaxNodeType.ANGLE,
            "minecraft:block_state" => JMCSyntaxNodeType.BLOCK_STATE,
            "minecraft:color" => JMCSyntaxNodeType.COLOR,
            "minecraft:column_pos" => JMCSyntaxNodeType.VEC2,
            "minecraft:entity" => JMCSyntaxNodeType.ENTITY,
            "minecraft:entity_anchor" => JMCSyntaxNodeType.ENTITY_ANCHOR,
            "minecraft:float_range" => JMCSyntaxNodeType.FLOAT_RANGE,
            "minecraft:function" => JMCSyntaxNodeType.FUNCTION_CALL,
            _ => JMCSyntaxNodeType.UNKNOWN
        };

        private bool Expect(string value, CommandNode node)
        {
            var parser = node.Parser;
            var prop = node.Properties;

            return parser switch
            {
                "brigadier:bool" => ExpectBool(value),
                "brigadier:double" => ExpectDouble(value, prop),
                "brigadier:float" => ExpectFloat(value, prop),
                "brigadier:integer" => ExpectInt(value, prop),
                "brigadier:long" => ExpectLong(value, prop),
                "brigadier:string" => ExpectString(value, prop!),
                "minecraft:vec2" => ExpectVec2(value),
                "minecraft:vec3" => ExpectVec3(value),
                "minecraft:angle" => ExpectAngle(value),
                "minecraft:block_state" => ExpectBlock(value),
                "minecraft:color" => ExpectColor(value),
                "minecraft:column_pos" => ExpectVec2(value),
                "minecraft:dimension" => ExpectResourceLocation(value),
                "minecraft:entity" => ExpectEntity(value, prop),
                "minecraft:entity_anchor" => ExpectEntityAnchor(value),
                "minecraft:float_range" => ExpectFloatRange(value),
                "minecraft:function" => ExpectFunction(value),
                _ => false
            };
        }

        private CommandNode? GetCommandNode(string cmd)
        {
            if (!Started)
            {
                Started = true;
                return CommandTree.Nodes.FirstOrDefault(v => v.Key == cmd).Value;
            }

            if (CurrentNode != null && CurrentNode.Children == null && CurrentNode.Executable == null)
            {
                var value = CommandTree.Nodes.FirstOrDefault(v => v.Key == cmd).Value;
                if (value != null) return value;
                //function and vars
                var isFunc = ExpectFunction(cmd);
                Nodes.Add(GetSyntaxNode(cmd, JMCSyntaxNodeType.FUNCTION_CALL));
                if (isFunc) return null;
            }
                


            if (CurrentNode == null || CurrentNode.Children == null || CurrentNode.Children.Count == 0)
                return null;
            var children = CurrentNode.Children;

            var arr = children.ToArray().AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var child = ref arr[i];
                var node = child.Value;

                if (Expect(cmd, node) || (child.Key == cmd && child.Value.Type == CommandNodeType.LITERAL))
                    return node;
            }
            return null;
        }

        private string ReadToEnd() => CommandString[Index..];
        private string Read()
        {
            StartReadIndex = Index;
            var s = string.Empty;
            do
            {
                s += CurrentChar switch
                {
                    '{' => ReadCP(),
                    '[' => ReadMP(),
                    '(' => ReadParen(),
                    '"' => ReadQuote(),
                    _ => ""
                };
                s += End ? "" : CurrentChar;
                if (CurrentChar == '.') Skip();
                else Next();
            } while (!char.IsWhiteSpace(CurrentChar) && !End);
            Index--;
            Skip();
            return s;
        }

        private string ReadQuote()
        {
            var s = string.Empty;
            do
            {
                s += CurrentChar;
                Next();
            } while (CurrentChar != '"' && !char.IsWhiteSpace(CurrentChar) && !End);
            return s;
        }

        private string ReadCP()
        {
            var s = string.Empty;
            var counter = 0;
            do
            {
                counter += CurrentChar switch
                {
                    '{' => 1,
                    '}' => -1,
                    _ => 0
                };
                s += CurrentChar;
                Next();
            } while (!char.IsWhiteSpace(CurrentChar) && !End);
            return s;
        }

        private string ReadMP()
        {
            var s = string.Empty;
            var counter = 0;
            do
            {
                counter += CurrentChar switch
                {
                    '[' => 1,
                    ']' => -1,
                    _ => 0
                };
                s += CurrentChar;
                Next();
            } while (!char.IsWhiteSpace(CurrentChar) && !End);
            return s;
        }

        private string ReadParen()
        {
            var s = string.Empty;
            var counter = 0;
            do
            {
                counter += CurrentChar switch
                {
                    '(' => 1,
                    ')' => -1,
                    _ => 0
                };
                s += CurrentChar;
                Next();
            } while (!char.IsWhiteSpace(CurrentChar) && !End);
            return s;
        }

        private void Next() => Index++;
        private void Skip()
        {
            do Index++;
            while (char.IsWhiteSpace(CurrentChar));
        }
    }
}
