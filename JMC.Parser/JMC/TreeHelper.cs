using JMC.Parser.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Parser.JMC
{
    internal static class TreeHelper
    {
        public static Position ToPosition(this int offset, string text)
        {
            var splitTextSpan = text.Split(Environment.NewLine).AsSpan();
            var textCounter = offset;

            int i = 0;
            for (; i < splitTextSpan.Length; i++)
            {
                ref var lineText = ref splitTextSpan[i];
                lineText += Environment.NewLine;
                textCounter -= lineText.Length;
                if (textCounter < 0)
                {
                    textCounter += lineText.Length;
                    break;
                }
            }

            return new Position(i, textCounter);
        }

        public static ParseQuery AsParseQuery(this SyntaxTree syntaxTree, int index = 0) => new(syntaxTree, index);

        public static string ToTokenString(this SyntaxNodeType nodeType) => nodeType switch
        {
            SyntaxNodeType.OpeningCurlyParenthesis => "{",
            SyntaxNodeType.ClosingCurlyParenthesis => "}",
            SyntaxNodeType.Literal => "literal",
            _ => nodeType.ToString(),
        };

        public static Span<char> ToCharSpan(this string value, int index = 0) => value.ToCharArray().AsSpan(index);

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f) =>
            e.SelectMany(c => f(c).Flatten(f)).Concat(e);
    }
}
