using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private static readonly ImmutableDictionary<JMCSyntaxNodeType, Regex> TokenPatterns = new Dictionary<JMCSyntaxNodeType, Regex>()
        {
            [JMCSyntaxNodeType.COMMENT] = new Regex(@"^\/\/.*$", _regexOptions),
            [JMCSyntaxNodeType.NUMBER] = new Regex(@"^(-?\d*\.?\d+)$", _regexOptions),
            [JMCSyntaxNodeType.STRING] = new Regex(@"^([""\'].*[""\'])$", _regexOptions),
            [JMCSyntaxNodeType.MULTILINE_STRING] = new Regex(@"^\`(?:.|\s)*\`$", _regexOptions),
            [JMCSyntaxNodeType.VARIABLE] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*$", _regexOptions),
            [JMCSyntaxNodeType.VARIABLE_CALL] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
            [JMCSyntaxNodeType.LITERAL] = new Regex(@"^(?![0-9])\S+$", _regexOptions),
            [JMCSyntaxNodeType.SELECTOR] = new Regex(@"^@[parse]$", _regexOptions),
            [JMCSyntaxNodeType.TILDE] = new Regex(@"^~$", _regexOptions),
            [JMCSyntaxNodeType.CARET] = new Regex(@"^\^$", _regexOptions)
        }.ToImmutableDictionary();

        private static readonly ImmutableArray<JMCSyntaxNodeType> OperatorsAssignTokens = [
            JMCSyntaxNodeType.OP_INCREMENT,
            JMCSyntaxNodeType.OP_DECREMENT,
            JMCSyntaxNodeType.OP_PLUSEQ,
            JMCSyntaxNodeType.OP_SUBSTRACTEQ,
            JMCSyntaxNodeType.OP_MULTIPLYEQ,
            JMCSyntaxNodeType.OP_DIVIDEEQ,
            JMCSyntaxNodeType.OP_NULLCOALE,
            JMCSyntaxNodeType.OP_SUCCESS,
            JMCSyntaxNodeType.EQUAL_TO,
            JMCSyntaxNodeType.OP_SWAP
        ];

        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;
    }
}
