using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JMC.Extension.Server.Lexer.JMC
{
    internal partial class JMCSyntaxTree
    {
        #region Patterns
        private static readonly SyntaxNodeType[] ClassPattern =
        [
            SyntaxNodeType.LITERAL,
            SyntaxNodeType.LCP
        ];
        private static readonly SyntaxNodeType[] FunctionPattern =
        [
            SyntaxNodeType.LITERAL,
            SyntaxNodeType.LPAREN,
            SyntaxNodeType.RPAREN,
            SyntaxNodeType.LCP,
        ];
        private static readonly SyntaxNodeType[] ImportPattern =
        [
            SyntaxNodeType.STRING,
            SyntaxNodeType.SEMI
        ];

        private static readonly SyntaxNodeType[] FunctionCallPattern =
        [
            SyntaxNodeType.LITERAL,
            SyntaxNodeType.LPAREN,
        ];

        private static readonly ImmutableDictionary<SyntaxNodeType, Regex> TokenPatterns = new Dictionary<SyntaxNodeType, Regex>()
        {
            [SyntaxNodeType.COMMENT] = new Regex(@"^\/\/.*$", _regexOptions),
            [SyntaxNodeType.NUMBER] = new Regex(@"^(\b-?\d*\.?\d+\b)$", _regexOptions),
            [SyntaxNodeType.STRING] = new Regex(@"^([""\'].*[""\'])$", _regexOptions),
            [SyntaxNodeType.MULTILINE_STRING] = new Regex(@"^\`(?:.|\s)*\`$", _regexOptions),
            [SyntaxNodeType.VARIABLE] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*$", _regexOptions),
            [SyntaxNodeType.VARIABLE_CALL] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
            [SyntaxNodeType.LITERAL] = new Regex(@"^(?![0-9])\S+$", _regexOptions),
            [SyntaxNodeType.SELECTOR] = new Regex(@"^@[parse]$", _regexOptions)
        }.ToImmutableDictionary();
        #endregion

        #region Regex
        private static readonly Regex SPLIT_PATTERN = SplitPatternRegex();
        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;
        [GeneratedRegex(@"(\/\/.*)|(\`(?:.|\s)*\`)|(-?\d*\.?\b\d+[lbs]?\b)|(\.\.\d+)|([""\'].*[""\'])|(\s|\;|\{|\}|\[|\]|\(|\)|\|\||&&|==|!=|[\<\>]\=|[\<\>]|\>\<|\+\+|\-\-|\?=|\?\?\=|!|,|:|\=\>|[\+\-\*\%\/]\=|[\+\-\*\%\/]|\=)")]
        private static partial Regex SplitPatternRegex();
        #endregion
    }
}
