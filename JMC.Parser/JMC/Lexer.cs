using System.Collections.Immutable;

namespace JMC.Parser.JMC
{
    internal class Lexer(string text)
    {
        private static readonly ImmutableArray<char> ValidChars = [.. "(){}[],:;<>=!"];
        private static readonly ImmutableArray<char> ValidOperatorChars = [.. "+-*/%"];
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
            "%=",
            "?="
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

        private char NextPeekChar
        {
            get
            {
                return Index + 2 >= RawText.Length ? '\0' : RawText[Index + 2];
            }
        }
        public IEnumerable<string> StartLexing()
        {
            var currentString = string.Empty;
            var lexResult = new List<string>();
            var rawTextCharSpan = RawText.ToCharSpan();
            for (; Index < rawTextCharSpan.Length; Index++)
            {
                ref var currentChar = ref rawTextCharSpan[Index];
                var twoChars = new string(new char[] { currentChar, PeekChar });
                var threeChars = new string(new char[] { currentChar, PeekChar, NextPeekChar });
                if (threeChars == "??=")
                {
                    lexResult.Add(currentString);
                    lexResult.Add(threeChars);
                    Index += 2;
                    currentString = string.Empty;
                }
                else if (ValidTwoChars.Contains(twoChars))
                {
                    lexResult.Add(currentString);
                    lexResult.Add(twoChars);
                    Index++;
                    currentString = string.Empty;
                }
                else if (twoChars == "//")
                {
                    var temp = string.Empty;
                    for (; Index < rawTextCharSpan.Length; Index++)
                    {
                        ref var c = ref rawTextCharSpan[Index];
                        temp += c;
                        if (temp.EndsWith(Environment.NewLine)) break;
                    }
                    Index -= Environment.NewLine.Length;
                    var end = temp.Length - Environment.NewLine.Length;
                    temp = temp[..end];
                    lexResult.Add(temp);
                }
                else if (currentChar == '`')
                {
                    var temp = "`";
                    Index++;
                    for (; Index < rawTextCharSpan.Length; Index++)
                    {
                        ref var c = ref rawTextCharSpan[Index];
                        temp += c;
                        if (c == '`') break;
                    }
                    lexResult.Add(temp);
                }
                else if (currentChar == '"')
                {
                    var temp = "\"";
                    Index++;
                    for (; Index < rawTextCharSpan.Length; Index++)
                    {
                        ref var c = ref rawTextCharSpan[Index];
                        temp += c;
                        if (c == '"') break;
                    }
                    lexResult.Add(temp);
                }
                else if (currentString.StartsWith('$') && currentChar == '.')
                {
                    lexResult.Add(currentString);
                    lexResult.Add(".");
                    currentString = string.Empty;
                }
                else if (char.IsWhiteSpace(currentChar) || ValidChars.Contains(currentChar) || ValidOperatorChars.Contains(currentChar))
                {
                    if (currentString != string.Empty)
                        lexResult.Add(currentString);
                    lexResult.Add(currentChar.ToString());
                    currentString = string.Empty;
                }
                else currentString += currentChar;
            }

            return lexResult;
        }
    }
}
