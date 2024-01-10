using JMC.Parser.JMC.Types;
using System.Text;

namespace JMC.Parser.JMC
{
    internal class JsonParser(string jsonString)
    {
        private string JsonString { get; set; } = jsonString;
        public static SyntaxNode Parse(string jsonString, Encoding encoding)
        {
            var rootNode = new SyntaxNode()
            {
                NodeType = SyntaxNodeType.Json
            };

            return rootNode;
        }

        public static SyntaxNode Parse(string jsonString) => Parse(jsonString, Encoding.UTF8);
        public SyntaxNode Parse() => Parse(JsonString, Encoding.UTF8);
    }
}
