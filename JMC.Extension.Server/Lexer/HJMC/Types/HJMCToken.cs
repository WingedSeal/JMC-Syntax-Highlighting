namespace JMC.Extension.Server.Lexer.HJMC.Types
{
    public class HJMCToken
    {
        public HJMCTokenType Type { get; set; } = HJMCTokenType.UNKNOWN;
        public List<string> Values { get; set; } = new List<string>();
        public HJMCToken() { }
        public HJMCToken(HJMCTokenType type, List<string> values)
        {
            Type = type;
            Values = values;
        }

    }
}
