using JMC.Parser.JMC.Types;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JMC.Parser.JMC
{
    internal partial class SyntaxTree
    {
        //private static readonly ImmutableDictionary<JMCSyntaxNodeType, Regex> TokenPatterns = new Dictionary<JMCSyntaxNodeType, Regex>()
        //{
        //    [JMCSyntaxNodeType.VariableCall] = new Regex(@"^\$[a-zA-Z_][0-9a-zA-Z_]*\s*\.", _regexOptions),
        //}.ToImmutableDictionary();

        private static readonly ImmutableArray<char> LiteralChars = [.. "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_."];

        private static readonly ImmutableArray<SyntaxNodeType> OperatorsAssignTokens = [
            SyntaxNodeType.IncrementOperator,
            SyntaxNodeType.DecrementOperator,
            SyntaxNodeType.PlusEqualOperator,
            SyntaxNodeType.SubtractEqualOperator,
            SyntaxNodeType.MultiplyEqualOperator,
            SyntaxNodeType.RemainderEqualOperator,
            SyntaxNodeType.DivideEqualOperator,
            SyntaxNodeType.NullcoaleOperator,
            SyntaxNodeType.SuccessOperator,
            SyntaxNodeType.EqualTo,
            SyntaxNodeType.SwapOperator,
        ];

        private static readonly ImmutableArray<SyntaxNodeType> OperatorTokens = [
            SyntaxNodeType.PlusOperator,
            SyntaxNodeType.SubtractOperator,
            SyntaxNodeType.MultiplyOperator,
            SyntaxNodeType.RemainderOperator,
            SyntaxNodeType.DivideOperator,
        ];

        private static readonly ImmutableArray<SyntaxNodeType> ConditionalOperatorTokens = [
            SyntaxNodeType.Equal,
            SyntaxNodeType.EqualTo,
            SyntaxNodeType.GreaterThan,
            SyntaxNodeType.GreaterThanEqual,
            SyntaxNodeType.LessThan,
            SyntaxNodeType.LessThanEqual,
        ];

        private static readonly ImmutableArray<SyntaxNodeType> LogicOperatorTokens = [
            SyntaxNodeType.Or,
            SyntaxNodeType.And,
            SyntaxNodeType.Not,
            SyntaxNodeType.OpeningParenthesis,
        ];

        private static readonly ImmutableArray<SyntaxNodeType> LogicTokens = [
            SyntaxNodeType.Or,
            SyntaxNodeType.And,
            SyntaxNodeType.Not,
            SyntaxNodeType.Equal,
            SyntaxNodeType.EqualTo,
            SyntaxNodeType.GreaterThan,
            SyntaxNodeType.GreaterThanEqual,
            SyntaxNodeType.LessThan,
            SyntaxNodeType.LessThanEqual,
        ];

        private static readonly ImmutableArray<string> JSONFileTypes = [
            "advancements", "item_modifiers", "loot_tables", "tags", "recipes", "dimensions", "dimension_types", "predicates"
        ];

        private static readonly RegexOptions _regexOptions = RegexOptions.Compiled;
    }
}
