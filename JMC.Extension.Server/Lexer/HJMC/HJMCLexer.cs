using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JMC.Extension.Server.Lexer.HJMC.Types;

namespace JMC.Extension.Server.Lexer.HJMC
{
    public class HJMCLexer
    {
        public List<HJMCToken> Tokens = new();
        private string RawText { get; set; }
        private string[] SplitedText { get; set; }
        public HJMCLexer(string text)
        {
            RawText = text;
            SplitedText = text.Split(Environment.NewLine);

            var arr = SplitedText.AsSpan();
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
            var splited = new Regex(@"(#\S+)|("".*"")|(\s)").Split(line);
            var headerType = GetHeaderType(splited[0]);

            return new()
            {
                Type = headerType,
                Values = splited[2..].ToList(),
            };
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
