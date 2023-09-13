using JMC.Extension.Server.Lexer.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace JMC.Extension.Server.Lexer.JMC
{
    public partial class JMCLexer
    {
        public static Regex SPLIT_PATTERN =
            new(@"(\/\/.*)|(\`(?:.|\s)*\`)|(-?\d*\.?\b\d+[lbs]?\b)|(\.\.\d+)|([""\'].*[""\'])|(\s|\;|\{|\}|\[|\]|\(|\)|\|\||&&|==|!=|[\<\>]\=|[\<\>]|!|,|:|\=\>|[\+\-\*\%\/]\=|[\+\-\*\%\/]|\=)");

        /// <summary>
        /// Tokens of the <see cref="JMCLexer"/>
        /// </summary>
        /// <remarks>
        /// DO NOT use this for accessing Variables or Functions,
        /// Use <see cref="Variables"/> or <see cref="FunctionCalls"/> to access datas
        /// </remarks>
        public List<JMCToken> Tokens { get; set; } = new();
        public string RawText { get; set; }

        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;

        /// <summary>
        /// Pattern for general tokens
        /// </summary>
        private static readonly ImmutableDictionary<JMCTokenType, Regex> TokenPatterns = new Dictionary<JMCTokenType, Regex>()
        {
            [JMCTokenType.COMMENT] = new Regex(@"^\/\/.*$", _regexOptions),
            [JMCTokenType.NUMBER] = new Regex(@"^(\b-?\d*\.?\d+\b)$", _regexOptions),
            [JMCTokenType.STRING] = new Regex(@"^([""\'].*[""\'])$", _regexOptions),
            [JMCTokenType.MULTILINE_STRING] = new Regex(@"^\`(?:.|\s)*\`$", _regexOptions),
            [JMCTokenType.VARIABLE] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*$", _regexOptions),
            [JMCTokenType.VARIABLE_CALL] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
            [JMCTokenType.LITERAL] = new Regex(@"^(?![0-9])\S+$", _regexOptions)
        }.ToImmutableDictionary();

        /// <summary>
        /// Pattern for command tokens
        /// </summary>
        private static readonly ImmutableDictionary<JMCTokenType, Regex> CommandTokenPatterns = new Dictionary<JMCTokenType, Regex>()
        {
            [JMCTokenType.COMMAND_VARIABLE] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*$", _regexOptions),
            [JMCTokenType.COMMAND_VARIABLE_CALL] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
            [JMCTokenType.COMMAND_INT_OR_LONG] = new Regex(@"^-?\d+$", _regexOptions),
            [JMCTokenType.COMMAND_FLOAT_OR_DOUBLE] = new Regex(@"^-?\d*\.?\d+[lbs]?$", _regexOptions),
            [JMCTokenType.COMMAND_LITERAL] = new Regex(@"^\w+$", _regexOptions),
            [JMCTokenType.COMMAND_VALUE] = new Regex(@"^\S+$", _regexOptions)
        }.ToImmutableDictionary();

        private const string CONDITION_TOKEN = "CONDITION";
        private const string COMMAND_TOKEN = "COMMAND";

        /// <summary>
        /// all type of variable tokens
        /// </summary>
        /// <remarks>
        /// Use this when searching for variables
        /// </remarks>
        public static readonly JMCTokenType[] VariableTypes = new JMCTokenType[]
        {
            JMCTokenType.VARIABLE, JMCTokenType.VARIABLE_CALL,
            JMCTokenType.COMMAND_VARIABLE, JMCTokenType.COMMAND_VARIABLE_CALL,
            JMCTokenType.CONDITION_VARIABLE
        };

        /// <summary>
        /// variable type for $xxx.get()
        /// </summary>
        public static readonly JMCTokenType[] VariableCallTypes = new JMCTokenType[]
        {
            JMCTokenType.VARIABLE_CALL, JMCTokenType.COMMAND_VARIABLE_CALL
        };

        /// <summary>
        /// variable type for $xxx
        /// </summary>
        public static readonly JMCTokenType[] NormalVariableTypes = new JMCTokenType[]
        {
            JMCTokenType.VARIABLE, JMCTokenType.COMMAND_VARIABLE, JMCTokenType.CONDITION_VARIABLE
        };

        /// <summary>
        /// initialize the JMC lexer
        /// </summary>
        /// <param name="text">raw text</param>
        public JMCLexer(string text)
        {
            RawText = text;
            InitTokens();
        }

        #region Query

        public IEnumerable<JMCToken> Variables => Tokens.FindAll(v => VariableTypes.Contains(v.TokenType));
        public IEnumerable<JMCToken> FunctionCalls
        {
            get
            {
                for (var i = 0; i < Tokens.Count; i++)
                {
                    if (i + 1 >= Tokens.Count)
                        yield break;

                    var token = Tokens[i];
                    var next = Tokens[i + 1];
                    if (token.TokenType == JMCTokenType.LITERAL &&
                        next.TokenType == JMCTokenType.LPAREN)
                    {
                        yield return token;
                    }
                }
            }
        }

        public IEnumerable<JMCToken> FunctionDefines
        {
            get
            {
                for (var i = 0; i < Tokens.Count; i++)
                {
                    if (i - 1 == -1)
                        continue;

                    var token = Tokens[i];
                    var pre = Tokens[i - 1];
                    if (token.TokenType == JMCTokenType.LITERAL &&
                        pre.TokenType == JMCTokenType.FUNCTION)
                    {
                        yield return token;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// initialize the tokens of <see cref="RawText"/>
        /// </summary>
        public void InitTokens()
        {
            Tokens.Clear();

            var splitedText = SPLIT_PATTERN.Split(RawText);
            var trimmedText = splitedText.Select(x => x.Trim()).ToArray();
            var currentPos = 0;

            var arr = trimmedText.AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var value = ref arr[i];
                if (string.IsNullOrEmpty(value))
                {
                    currentPos += splitedText[i].Length;
                    continue;
                }
                var token = Tokenize(value, currentPos);
                if (token != null)
                {
                    Tokens.Add(token);
                }
                currentPos += splitedText[i].Length;
            }
            FormatFunctions();
        }

        /// <summary>
        /// make function in a class to have a separator for its value,
        /// <code>
        /// "test class.test"
        /// </code>
        /// </summary>
        public void FormatFunctions()
        {
            for (var i = 0; i < Tokens.Count; i++)
            {
                if (i - 1 == -1) continue;
                var token = Tokens[i];
                if (token.TokenType == JMCTokenType.LITERAL &&
                    Tokens[i - 1].TokenType == JMCTokenType.FUNCTION)
                {
                    foreach (var range in GetClassRanges())
                    {
                        if (range.Value.Contains(token.Range.Start))
                        {
                            token.Value += $" {range.Key}.{token.Value}";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// get ranges of the class start and end
        /// </summary>
        /// <returns>
        /// list of <see cref="KeyValuePair"/> contains class name by <see cref="string"/> in Key and 
        /// <see cref="Range"/> in Value
        /// </returns>
        public IEnumerable<KeyValuePair<string, Range>> GetClassRanges()
        {
            for (var i = 0; i < Tokens.Count; i++)
            {
                var token = Tokens[i];
                if (token.TokenType == JMCTokenType.CLASS)
                {
                    var parenCount = 0;
                    var className = Tokens[i + 1].Value;
                    for (i += 2; i < Tokens.Count; i++)
                    {
                        var c = Tokens[i];
                        if (c.TokenType == JMCTokenType.LCP) parenCount++;
                        else if (c.TokenType == JMCTokenType.RCP) parenCount--;
                        if (parenCount == 0)
                        {
                            var endPos = c.Offset + c.Value.Length;
                            var startPos = token.Offset;
                            var end = OffsetToPosition(endPos, RawText);
                            var start = OffsetToPosition(startPos, RawText);
                            yield return new(className, new Range(start, end));
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// modify the text of the <see cref="JMCLexer"/>
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
        /// Tokenize a <see cref="string"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos">offset of the text</param>
        /// <param name="parsedTokens">previous tokens</param>
        /// <returns></returns>
        public JMCToken? Tokenize(string text, int pos)
        {
            var start = OffsetToPosition(pos, RawText);
            var end = OffsetToPosition(pos + text.Length, RawText);
            var range = new Range(start, end);
            try
            {
                var preType = Tokens[^1].TokenType;
                var type = GetTokenType(text);
                if (Tokens[^2].TokenType == JMCTokenType.IF ||
                    preType.ToString().StartsWith(CONDITION_TOKEN, StringComparison.CurrentCulture))
                {
                    type = GetIfTokenType(type);
                }
                else if (preType.ToString().StartsWith(COMMAND_TOKEN, StringComparison.CurrentCulture))
                {
                    type = GetCommandTokenType(text);
                }
                return new()
                {
                    Offset = pos,
                    Range = range,
                    TokenType = type,
                    Value = text,
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                var type = GetTokenType(text);
                return new()
                {
                    Offset = pos,
                    Range = range,
                    TokenType = type,
                    Value = text,
                };
            }
        }

        /// <summary>
        /// get a token by <see cref="Position"/>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public JMCToken? GetJMCToken(Position pos)
        {
            var arr = CollectionsMarshal.AsSpan(Tokens);
            for (var i = 0; i < Tokens.Count; i++)
            {
                ref var c = ref arr[i];
                if (i + 1 >= Tokens.Count)
                {
                    var range = new Range(c.Range.Start, c.Range.End);
                    if (range.Contains(pos))
                    {
                        return c;
                    }
                }
                var next = Tokens[i + 1];
                if (next != null)
                {
                    var range = new Range(c.Range.Start, next.Range.Start);
                    if (range.Contains(pos))
                    {
                        return c;
                    }
                }
            }
            return null;
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

        /// <summary>
        /// get type of the <see cref="string"/> of a command
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static JMCTokenType GetCommandTokenType(string text)
        {
            switch (text)
            {
                case "{": return JMCTokenType.COMMAND_LCP;
                case "}": return JMCTokenType.COMMAND_RCP;
                case "[": return JMCTokenType.COMMAND_LMP;
                case "]": return JMCTokenType.COMMAND_RMP;
                case "(": return JMCTokenType.COMMAND_LPAREN;
                case ")": return JMCTokenType.COMMAND_RPAREN;
                case ";": return JMCTokenType.SEMI;
                case "^": return JMCTokenType.COMMAND_CARET;
                case "~": return JMCTokenType.COMMAND_TILDE;
                default: break;
            }

            try
            {
                var result = CommandTokenPatterns.First(v => v.Value.IsMatch(text)).Key;
                return result;
            }
            catch (InvalidOperationException)
            {
                return JMCTokenType.COMMAND_UNKNOWN;
            }
        }

        /// <summary>
        /// get type of the <see cref="string"/>
        /// </summary>
        /// <param name="text">string that required to be parsed</param>
        /// <returns><see cref="JMCTokenType"/> or <see cref="JMCTokenType.UNKNOWN"/> if it can't recognize</returns>
        /// <param name="lastToken"></param>
        public static JMCTokenType GetTokenType(string text)
        {
            switch (text)
            {
                //keyword
                case "function":
                    return JMCTokenType.FUNCTION;
                case "switch":
                    return JMCTokenType.SWITCH;
                case "case":
                    return JMCTokenType.CASE;
                case "if":
                    return JMCTokenType.IF;
                case "else":
                    return JMCTokenType.ELSE;
                case "do":
                    return JMCTokenType.DO;
                case "while":
                    return JMCTokenType.WHILE;
                case "new":
                    return JMCTokenType.NEW;
                case "class":
                    return JMCTokenType.CLASS;

                //brackets
                case "(":
                    return JMCTokenType.LPAREN;
                case ")":
                    return JMCTokenType.RPAREN;
                case "{":
                    return JMCTokenType.LCP;
                case "}":
                    return JMCTokenType.RCP;
                case "[":
                    return JMCTokenType.LMP;
                case "]":
                    return JMCTokenType.RMP;
                case ":":
                    return JMCTokenType.COLON;
                case ";":
                    return JMCTokenType.SEMI;

                //boolean ops
                case "==":
                    return JMCTokenType.OP_EQUAL;
                case "!=":
                    return JMCTokenType.NOT_EQUAL;
                case ">=":
                    return JMCTokenType.GREATER_OR_EQUAL;
                case "<=":
                    return JMCTokenType.LESS_OR_EQUAL;
                case ">":
                    return JMCTokenType.GREATER_THEN;
                case "<":
                    return JMCTokenType.LESS_THEN;
                case "||":
                    return JMCTokenType.OR;
                case "&&":
                    return JMCTokenType.AND;

                //operators equal
                case "+=":
                    return JMCTokenType.PLUS_EQUAL;
                case "-=":
                    return JMCTokenType.MINUS_EQUAL;
                case "/=":
                    return JMCTokenType.DIVIDE_EQUAL;
                case "%=":
                    return JMCTokenType.REMINDER_EQUAL;
                case "*=":
                    return JMCTokenType.MULTIPLY_EQUAL;
                case "=":
                    return JMCTokenType.OP_EQUAL;

                //operators
                case "+":
                    return JMCTokenType.PLUS;
                case "-":
                    return JMCTokenType.MINUS;
                case "*":
                    return JMCTokenType.MULTIPLY;
                case "/":
                    return JMCTokenType.DIVIDE;
                case "%":
                    return JMCTokenType.REMAINDER;

                //others
                case "=>":
                    return JMCTokenType.ARROW_FUNC;

                default:
                    break;
            }

            //special cases
            try
            {
                var result = TokenPatterns.First(v => v.Value.IsMatch(text)).Key;
                return result == JMCTokenType.LITERAL
                    && ExtensionData.CommandData.RootCommands.Contains(text)
                    ? JMCTokenType.COMMAND_LITERAL
                    : result;
            }
            catch (Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
            {
                return JMCTokenType.UNKNOWN;
            }
            catch (NullReferenceException)
            {
                var result = TokenPatterns.First(v => v.Value.IsMatch(text)).Key;
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static JMCTokenType GetIfTokenType(JMCTokenType type)
        {
            switch (type)
            {
                case JMCTokenType.RPAREN:
                    return JMCTokenType.RPAREN;
                case JMCTokenType.VARIABLE: 
                    return JMCTokenType.CONDITION_VARIABLE;
                case JMCTokenType.LITERAL:
                    return JMCTokenType.CONDITION_LITERAL;
                default:
                    break;
            }

            return JMCTokenType.CONDITION_UNKNOWN;
        }

        #region AsyncMethods
        /// <summary>
        /// <inheritdoc cref="ChangeRawText(TextDocumentContentChangeEvent)"/>
        /// </summary>
        /// <returns></returns>
        public async Task ChangeRawTextAsync(TextDocumentContentChangeEvent change) => await Task.Run(() => ChangeRawText(change));

        /// <summary>
        /// <inheritdoc cref="InitTokens"/>
        /// </summary>
        /// <returns></returns>
        public async Task InitTokensAsync() => await Task.Run(InitTokens);
        #endregion
    }
}
