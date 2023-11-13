using System.Collections.Immutable;

namespace JMC.Parser.JMC
{
    internal class JMCLexer(string text)
    {
        private static readonly ImmutableArray<char> ValidChars = [.. "(){}[],:;<>=!"];
        private static readonly ImmutableArray<char> ValidOpChars = [.. "+-*/%"];
        private static readonly ImmutableArray<string> ValidTwoChars = [
            "=>",
            "==",
            "!=",
            "&&",
            "||",
            ">=",
            "<=",
            "><",
            "--",
            "++",
            "+=",
            "-=",
            "*=",
            "/=",
            "%="
        ];
        public string RawText { get; private set; } = text;
        private int Index { get; set; } = 0;
        public IEnumerable<string> StartLexing()
        {
            var tempString = "";
            var strs = new List<string>();


            return strs;
        }

        private static bool IsValidChar(char ch) => ValidChars.Contains(ch);
    }
}
