using JMC.Extension.Server.Datas.Minecraft.Command;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Lexer.JMC
{
    internal static class CommandHelper
    {
        public static CommandNode? GetCommandNode(this JMCLexer lexer, Position position)
        {
            var commands = lexer.GetCommands().ToArray().AsSpan();
            for (var i = 0; i < commands.Length; i++)
            {
                ref var cmd = ref commands[i];
                var range = new Range(cmd.First().Range.Start, cmd.Last().Range.End);
                var token = lexer.GetJMCToken(position);
                if (!range.Contains(position) || token == null) continue;
                var startIndex = lexer.Tokens.IndexOf(cmd.First());
                var currentIndex = lexer.Tokens.IndexOf(token);
                for (var j = startIndex; j < currentIndex; j++)
                {
                    //TODO
                }
            }
            return null;
        }
    }
}
