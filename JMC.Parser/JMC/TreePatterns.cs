using JMC.Parser.JMC.Types;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JMC.Parser.JMC
{
    internal partial class JMCSyntaxTree
    {
        //private static readonly ImmutableDictionary<JMCSyntaxNodeType, Regex> TokenPatterns = new Dictionary<JMCSyntaxNodeType, Regex>()
        //{
        //    [JMCSyntaxNodeType.VariableCall] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
        //}.ToImmutableDictionary();

        private static readonly ImmutableArray<char> LiteralChars = [.. "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_."];

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

        private static readonly ImmutableArray<JMCSyntaxNodeType> LogicOperatorTokens = [
            JMCSyntaxNodeType.CompOr,
            JMCSyntaxNodeType.CompAnd,
            JMCSyntaxNodeType.CompNot,
            JMCSyntaxNodeType.LParen,
        ];

        private static readonly ImmutableArray<JMCSyntaxNodeType> LogicTokens = [
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

        private static readonly ImmutableArray<string> JSONFileTypes = [
            "advancements", "item_modifiers", "loot_tables", "tags", "recipes", "dimensions", "dimension_types", "predicates"
        ];

        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;
    }
}
