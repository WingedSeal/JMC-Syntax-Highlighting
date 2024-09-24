﻿using JMC.Parser.JMC.Types;
using JMC.Shared;
using JMC.Shared.Datas.Minecraft.Command;
using JMC.Shared.Datas.Minecraft.Command.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JMC.Parser.JMC.Command
{
    internal partial class CommandParser(string cmdString, int offset, string text)
    {
        private string CommandString { get; set; } = cmdString;
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
        public List<SyntaxNode> Nodes { get; private set; } = [];

        //Types
        private static ImmutableArray<string> Vec2Types => [
            "minecraft:vec2",
            "minecraft:column_pos"
        ];
        private static ImmutableArray<string> Vec3Types => [
            "minecraft:vec3",
            "minecraft:block_pos"
        ];
        public IEnumerable<SyntaxNode> ParseCommand()
        {
            while (CurrentChar != '\0')
            {
                var elem = CurrentNode?.Children?.ElementAtOrDefault(0).Value;
                if (elem != null &&
                    ((elem.Parser == "brigadier:string" &&
                    elem.Properties?.Type == "greedy") ||
                    elem.Parser == "minecraft:message"))
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
                if (!text.Substring(StartOffset, text.IndexOf('\n', StartOffset) - StartOffset).Contains(";")) 
                    Errors.Add(Index, $"Expect End");
            }
            else if (CurrentNode?.Executable == null && CurrentNode?.Children?.Keys != null)
            {
                var keys = CurrentNode?.Children?.Keys;
                var s = keys != null ? string.Join(" or ", keys.Select(v => $"'{v}'")) : "";
                Errors.Add(Index, $"Expect {s}");
            }

            return Nodes;
        }

        public IEnumerable<SyntaxNode> ParseIfCommand() // why doe we even keep this
        {
            var extend = "execute if ";
            CommandString = extend + CommandString;
            while (CurrentChar != '\0')
            {
                var elem = CurrentNode?.Children?.ElementAtOrDefault(0).Value;
                if (elem != null &&
                    ((elem.Parser == "brigadier:string" &&
                    elem.Properties?.Type == "greedy") ||
                    elem.Parser == "minecraft:message"))
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
            if (CurrentNode?.Executable == null)
            {
                var keys = CurrentNode?.Children?.Keys;
                var s = keys != null ? string.Join(" or ", keys.Select(v => $"'{v}'")) : "";
                Errors.Add(Index, $"Expect {s}");
            }

            return Nodes[2..];
        }
        private SyntaxNode GetSyntaxNode(string value)
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

            var node = new SyntaxNode();
            var start = (StartOffset + StartReadIndex).ToPosition(TreeText);
            var end = (StartOffset + StartReadIndex + Index - StartReadIndex - 1).ToPosition(TreeText);
            var range = new Range(start, end);

            var parser = CurrentNode.Parser;
            var type = CurrentNode.Type == CommandNodeType.LITERAL ?
                SyntaxNodeType.Literal :
                ToSyntaxNodeType(parser ?? "");

            node.Value = value;
            node.Range = range;
            node.NodeType = type;

            return node;
        }

        private SyntaxNode GetSyntaxNode(string value, SyntaxNodeType type)
        {
            if (CurrentNode == null) return new();

            var node = new SyntaxNode();
            var start = (StartOffset + StartReadIndex).ToPosition(TreeText);
            var end = (StartOffset + Index - 2).ToPosition(TreeText);
            var range = new Range(start, end);

            node.Value = value;
            node.Range = range;
            node.NodeType = type;

            return node;
        }

        private static SyntaxNodeType ToSyntaxNodeType(string parser) => parser switch
        {
            "brigadier:bool" => SyntaxNodeType.Bool,
            "brigadier:double" => SyntaxNodeType.Double,
            "brigadier:float" => SyntaxNodeType.Float,
            "brigadier:integer" => SyntaxNodeType.Int,
            "brigadier:long" => SyntaxNodeType.Long,
            "brigadier:string" => SyntaxNodeType.CommandString,
            "minecraft:block_pos" => SyntaxNodeType.Vec3,
            "minecraft:vec3" => SyntaxNodeType.Vec3,
            "minecraft:vec2" => SyntaxNodeType.Vec2,
            "minecraft:angle" => SyntaxNodeType.Angle,
            "minecraft:block_state" => SyntaxNodeType.BlockState,
            "minecraft:color" => SyntaxNodeType.Color,
            "minecraft:column_pos" => SyntaxNodeType.Vec2,
            "minecraft:entity" => SyntaxNodeType.Entity,
            "minecraft:entity_anchor" => SyntaxNodeType.EntityAnchor,
            "minecraft:float_range" => SyntaxNodeType.FloatRange,
            "minecraft:function" => SyntaxNodeType.FunctionCall,
            "minecraft:game_profile" => SyntaxNodeType.Entity,
            "minecraft:gamemode" => SyntaxNodeType.Literal,
            "minecraft:heightmap" => SyntaxNodeType.Literal,
            "minecraft:int_range" => SyntaxNodeType.IntRange,
            "minecraft:item_predicate" => SyntaxNodeType.ItemPredicate,
            "minecraft:item_slot" => SyntaxNodeType.ItemSlot,
            "minecraft:item_stack" => SyntaxNodeType.ItemStack,
            "minecraft:message" => SyntaxNodeType.Message,
            "minecraft:nbt_compound_tag" => SyntaxNodeType.NBT,
            "minecraft:nbt_path" => SyntaxNodeType.NBTPath,
            "minecraft:nbt_tag" => SyntaxNodeType.NbtTag,
            "minecraft:objective" => SyntaxNodeType.Literal,
            "minecraft:objective_criteria" => SyntaxNodeType.ObjectCriteria,
            "minecraft:operation" => SyntaxNodeType.Operator,
            "minecraft:particle" => SyntaxNodeType.Particle,
            "minecraft:resource" => SyntaxNodeType.Resource,
            "minecraft:resource_key" => SyntaxNodeType.Resource,
            "minecraft:resource_location" => SyntaxNodeType.Resource,
            "minecraft:resource_or_tag" => SyntaxNodeType.ResourceTag,
            "minecraft:resource_or_tag_key" => SyntaxNodeType.ResourceTag,
            "minecraft:rotation" => SyntaxNodeType.Rotation,
            "minecraft:score_holder" => SyntaxNodeType.Entity,
            "minecraft:scoreboard_slot" => SyntaxNodeType.Literal,
            "minecraft:swizzle" => SyntaxNodeType.Literal,
            "minecraft:team" => SyntaxNodeType.Team,
            "minecraft:template_mirror" => SyntaxNodeType.Literal,
            "minecraft:template_rotation" => SyntaxNodeType.Literal,
            "minecraft:time" => SyntaxNodeType.Time,
            "minecraft:uuid" => SyntaxNodeType.UUID,
            _ => SyntaxNodeType.Unknown
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
                "minecraft:game_profile" => ExpectGameProfile(value),
                "minecraft:gamemode" => ExpectGamemode(value),
                "minecraft:heightmap" => ExpectHeightmap(value),
                "minecraft:int_range" => ExpectIntRange(value),
                "minecraft:item_predicate" => ExpectItemPredicate(value),
                "minecraft:item_slot" => ExpectItemSlot(value),
                "minecraft:item_stack" => ExpectItemPredicate(value),
                "minecraft:nbt_compound_tag" => ExpectNBT(value),
                "minecraft:nbt_path" => true,
                "minecraft:nbt_tag" => true,
                "minecraft:objective" => ExpectObjective(value),
                "minecraft:objective_criteria" => ExpectObjectiveCriteria(value),
                "minecraft:operation" => ExpectOperation(value),
                "minecraft:particle" => ExpectParticle(value),
                "minecraft:resource" => ExpectResource(value),
                "minecraft:resource_key" => ExpectResource(value),
                "minecraft:resource_location" => ExpectResource(value),
                "minecraft:resource_or_tag" => ExpectResource(value) || value.StartsWith('#'),
                "minecraft:resource_or_tag_key" => ExpectResource(value) || value.StartsWith('#'),
                "minecraft:rotation" => ExpectVec2(value),
                "minecraft:score_holder" => ExpectEntity(value) || value == "*",
                "minecraft:scoreboard_slot" => ExpectScoreboardSlot(value),
                "minecraft:swizzle" => ExpectSwizzle(value),
                "minecraft:team" => ExpectTeam(value),
                "minecraft:template_mirror" => ExpectTemplateMirror(value),
                "minecraft:template_rotation" => ExpectTemplateRotation(value),
                "minecraft:time" => ExpectTime(value),
                "minecraft:uuid" => ExpectUUID(value),
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
                if (cmd.StartsWith('$') && VariableCallRegex().IsMatch(cmd))
                {
                    Nodes.Add(GetSyntaxNode(cmd, SyntaxNodeType.VariableCall));
                    return null;
                }
                else if (ExpectFunction(cmd))
                {
                    Nodes.Add(GetSyntaxNode(cmd, SyntaxNodeType.FunctionCall));
                    return null;
                }
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

        [GeneratedRegex(@"get\s*\((?:\s|.)*\)$", RegexOptions.Compiled)]
        private static partial Regex VariableCallRegex();
    }
}
