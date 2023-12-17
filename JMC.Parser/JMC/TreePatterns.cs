using JMC.Parser.JMC.Types;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        private static readonly ImmutableDictionary<JMCSyntaxNodeType, Regex> TokenPatterns = new Dictionary<JMCSyntaxNodeType, Regex>()
        {
            [JMCSyntaxNodeType.Comment] = new Regex(@"^\/\/.*$", _regexOptions),
            [JMCSyntaxNodeType.Number] = new Regex(@"^(-?\d*\.?\d+)$", _regexOptions),
            [JMCSyntaxNodeType.String] = new Regex(@"^([""\'].*[""\'])$", _regexOptions),
            [JMCSyntaxNodeType.MultilineString] = new Regex(@"^\`(?:.|\s)*\`$", _regexOptions),
            [JMCSyntaxNodeType.Variable] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*$", _regexOptions),
            [JMCSyntaxNodeType.VariableCall] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
            [JMCSyntaxNodeType.Literal] = new Regex(@"^(?![0-9])\S+$", _regexOptions),
            [JMCSyntaxNodeType.Selector] = new Regex(@"^@[parse]$", _regexOptions),
            [JMCSyntaxNodeType.Tilde] = new Regex(@"^~$", _regexOptions),
            [JMCSyntaxNodeType.Caret] = new Regex(@"^\^$", _regexOptions)
        }.ToImmutableDictionary();

        private static readonly ImmutableArray<JMCSyntaxNodeType> OperatorsAssignTokens = [
            JMCSyntaxNodeType.OpIncrement,
            JMCSyntaxNodeType.OpDecrement,
            JMCSyntaxNodeType.OpPlusEqual,
            JMCSyntaxNodeType.OpSubtractEqual,
            JMCSyntaxNodeType.OpMultiplyEqual,
            JMCSyntaxNodeType.OpRemainderEqual,
            JMCSyntaxNodeType.OpDivideEqual,
            JMCSyntaxNodeType.OpNullcoale,
            JMCSyntaxNodeType.OpSuccess,
            JMCSyntaxNodeType.EqualTo,
            JMCSyntaxNodeType.OpSwap,
        ];

        private static readonly ImmutableArray<JMCSyntaxNodeType> OperatorTokens = [
            JMCSyntaxNodeType.OpPlus,
            JMCSyntaxNodeType.OpSubtract,
            JMCSyntaxNodeType.OpMultiply,
            JMCSyntaxNodeType.OpRemainder,
            JMCSyntaxNodeType.OpDivide,
        ];

        private static readonly ImmutableArray<JMCSyntaxNodeType> ConditionalOperatorTokens = [
            JMCSyntaxNodeType.Equal,
            JMCSyntaxNodeType.EqualTo,
            JMCSyntaxNodeType.GreaterThan,
            JMCSyntaxNodeType.GreaterThanEqual,
            JMCSyntaxNodeType.LessThan,
            JMCSyntaxNodeType.LessThanEqual,
        ];

        private readonly ImmutableArray<JMCSyntaxNodeType> LogicOperatorTokens = [
            JMCSyntaxNodeType.CompOr,
            JMCSyntaxNodeType.CompAnd,
            JMCSyntaxNodeType.CompNot,
            JMCSyntaxNodeType.LParen,
        ];

        private readonly ImmutableArray<JMCSyntaxNodeType> LogicTokens = [
            JMCSyntaxNodeType.CompOr,
            JMCSyntaxNodeType.CompAnd,
            JMCSyntaxNodeType.CompNot,
            JMCSyntaxNodeType.Equal,
            JMCSyntaxNodeType.EqualTo,
            JMCSyntaxNodeType.GreaterThan,
            JMCSyntaxNodeType.GreaterThanEqual,
            JMCSyntaxNodeType.LessThan,
            JMCSyntaxNodeType.LessThanEqual,
        ];

        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;
    }
}
