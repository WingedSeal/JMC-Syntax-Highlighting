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
        private char PeekChar
        {
            get
            {
                return Index + 1 >= RawText.Length ? '\0' : RawText[Index + 1];
            }
        }
        public IEnumerable<string> StartLexing()
        {
            var tempString = string.Empty;
            var strs = new List<string>();
            var span = RawText.ToCharSpan();
            for (; Index < span.Length; Index++)
            {
                ref var ch = ref span[Index];
                var twoChars = new string(new char[] { ch, PeekChar });
                if (ValidTwoChars.Contains(twoChars))
                {
                    strs.Add(tempString);
                    strs.Add(twoChars);
                    Index++;
                    tempString = string.Empty;
                }
                else if (twoChars == "//")
                {
                    var temp = string.Empty;
                    for (; Index < span.Length; Index++)
                    {
                        ref var c = ref span[Index];
                        temp += c;
                        if (temp.EndsWith(Environment.NewLine)) break;
                    }
                    Index -= Environment.NewLine.Length;
                    var end = temp.Length - Environment.NewLine.Length;
                    temp = temp[..end];
                    strs.Add(temp);
                }
                else if (ch == '`')
                {
                    var temp = "`";
                    Index++;
                    for (; Index < span.Length; Index++)
                    {
                        ref var c = ref span[Index];
                        temp += c;
                        if (c == '`') break;
                    }
                    strs.Add(temp);
                }
                else if (ch == '"')
                {
                    var temp = "\"";
                    Index++;
                    for (; Index < span.Length; Index++)
                    {
                        ref var c = ref span[Index];
                        temp += c;
                        if (c == '"') break;
                    }
                    strs.Add(temp);
                }
                else if (char.IsWhiteSpace(ch) || ValidChars.Contains(ch) || ValidOpChars.Contains(ch))
                {
                    if (tempString != string.Empty)
                        strs.Add(tempString);
                    strs.Add(ch.ToString());
                    tempString = string.Empty;
                }
                else tempString += ch;
            }

            return strs;
        }
    }
}
