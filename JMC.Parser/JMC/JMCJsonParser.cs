using JMC.Parser.JMC.Types;
using System.Text;

namespace JMC.Parser.JMC
{
    internal class JMCJsonParser(string jsonString)
    {
        private string JsonString { get; set; } = jsonString;
        public static JMCSyntaxNode Parse(string jsonString, Encoding encoding)
        {
            var rootNode = new JMCSyntaxNode()
            {
                NodeType = JMCSyntaxNodeType.Json
            };

            return rootNode;
        }

        public static JMCSyntaxNode Parse(string jsonString) => Parse(jsonString, Encoding.UTF8);
        public JMCSyntaxNode Parse() => Parse(JsonString, Encoding.UTF8);
    }
}
