﻿namespace JMC.Parser.JMC.Types
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="nodeType"></param>
    /// <param name="value"></param>
    /// <param name="next"></param>
    /// <param name="range"></param>
    /// <param name="offset"></param>
    internal class JMCSyntaxNode(JMCSyntaxNodeType nodeType = JMCSyntaxNodeType.Unknown, string value = "", IEnumerable<JMCSyntaxNode>? next = null, Range? range = null, int offset = -1)
    {
        public Range? Range { get; set; } = range;
        public JMCSyntaxNodeType NodeType { get; set; } = nodeType;
        public IEnumerable<JMCSyntaxNode>? Next { get; set; } = next;
        public string? Value { get; set; } = value;
        public int Offset { get; set; } = offset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indent"></param>
        /// <param name="last"></param>
        public void PrintPretty(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine($"{NodeType} {Value} {Range}");

            if (Next != null)
                for (int i = 0; i < Next.Count(); i++)
                    Next.ElementAt(i).PrintPretty(indent, i == Next.Count() - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JMCSyntaxNode> ToFlattenNodes()
        {
            var current = new JMCSyntaxNode[] { this };
            if (Next == null) return current;
            else return current.Concat(Next.SelectMany(v => v.ToFlattenNodes()));
        }
    }
}