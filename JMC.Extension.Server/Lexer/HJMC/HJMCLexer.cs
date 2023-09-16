using JMC.Extension.Server.Lexer.HJMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace JMC.Extension.Server.Lexer.HJMC
{
    public class HJMCLexer
    {
        public static readonly Regex SplitPattern = new(@"(#\S+)|("".*"")|(\s)", RegexOptions.Compiled);

        public List<HJMCToken> Tokens = new();
        public string RawText { get; set; }
        public HJMCLexer(string text)
        {
            RawText = text;
            InitTokens();
        }

        public void InitTokens()
        {
            var splitedText = RawText.Split(Environment.NewLine);

            var arr = splitedText.AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var value = ref arr[i];

                var token = Tokenize(value);
                if (token != null)
                {
                    Tokens.Add(token);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static HJMCToken? Tokenize(string line)
        {
            var splited = SplitPattern.Split(line);
            var headerType = GetHeaderType(splited[0]);

            return new()
            {
                Type = headerType,
                Values = splited[2..].ToList(),
            };
        }

        /// <summary>
        /// modify the text of the lexer
        /// </summary>
        /// <param name="change"></param>
        public void ChangeRawText(TextDocumentContentChangeEvent change)
        {
            var r = change.Range;
            if (r == null)
                return;

            var start = PositionToOffset(r.Start, RawText);
            var end = PositionToOffset(r.End, RawText);

            RawText = RawText.Remove(start, end - start).Insert(start, change.Text);
        }

        /// <summary>
        /// Convert offset to <see cref="Position"/>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int PositionToOffset(Position pos, string text)
        {
            var line = pos.Line;

            var currentLine = 0;
            var offset = 0;


            var chars = text.ToCharArray().AsSpan();
            for (; offset < text.Length; offset++)
            {
                ref var c = ref chars[offset];
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (c == '\r')
                    {
                        offset++;
                        currentLine++;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (c == '\n') currentLine++;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (c == '\r') currentLine++;
                }
                if (currentLine == line) break;
            }

            return offset += pos.Character;
        }

        /// <summary>
        /// get <see cref="Position"/> by offset of the text
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="text">the whole document text</param>
        /// <returns><see cref="Position"/> of the offset</returns>
        public static Position OffsetToPosition(int offset, string text)
        {
            var line = 0;
            var col = 0;

            var arr = text.ToCharArray().AsSpan();
            for (var i = 0; i < offset; i++)
            {
                ref var value = ref arr[i];
                if (text[i] == '\n')
                {
                    line++;
                    col = 0;
                }
                else
                {
                    col++;
                }
            }

            return new Position(line, col);
        }

        private static HJMCTokenType GetHeaderType(string type)
        {
            return type switch
            {
                "#define" => HJMCTokenType.DEFINE,
                "#bind" => HJMCTokenType.BIND,
                "#credit" => HJMCTokenType.CREDIT,
                "#include" => HJMCTokenType.INCLUDE,
                "#command" => HJMCTokenType.COMMAND,
                "#del" => HJMCTokenType.DEL,
                "#override" => HJMCTokenType.OVERRIDE,
                "#uninstall" => HJMCTokenType.UNINSTALL,
                "#static" => HJMCTokenType.STATIC,
                "#nometa" => HJMCTokenType.NOMETA,
                _ => HJMCTokenType.UNKNOWN,
            };
        }
    }
}
