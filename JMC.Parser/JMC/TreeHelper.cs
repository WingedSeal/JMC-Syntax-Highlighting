using JMC.Parser.JMC.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Text;

namespace JMC.Parser.JMC
{
    internal static class TreeHelper
    {
        public static Position ToPosition(this int offset, string text)
        {
            var split = text.Split(Environment.NewLine).AsSpan();
            var textCounter = offset;

            int i = 0;
            for (; i < split.Length; i++)
            {
                ref var lineText = ref split[i];
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

        public static JMCParseQuery AsParseQuery(this JMCSyntaxTree syntaxTree, int index = 0) => new(syntaxTree, index);

        public static string ToTokenString(this JMCSyntaxNodeType nodeType)
        {
            return nodeType switch
            {
                JMCSyntaxNodeType.LCP => "{",
                JMCSyntaxNodeType.RCP => "}",
                JMCSyntaxNodeType.Literal => "literal",
                _ => nodeType.ToString(),
            };
        }

        public static Span<char> ToCharSpan(this string value, int index = 0)
        {
            var chars = value.ToCharArray();
            return chars.AsSpan(index);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f) =>
            e.SelectMany(c => f(c).Flatten(f)).Concat(e);
    }
}
