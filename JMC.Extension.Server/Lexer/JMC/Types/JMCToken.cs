using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace JMC.Extension.Server.Lexer.JMC.Types
{
    public class JMCToken
    {
        public JMCTokenType TokenType { get; set; } = JMCTokenType.UNKNOWN;
        public Range Range { get; set; } = new(-1, -1, -1, -1);
        public int Offset { get; set; } = -1;
        public string Value { get; set; } = string.Empty;
        public JMCToken() { }
        public JMCToken(JMCTokenType tokenType, Range range, string value, int offset)
        {
            TokenType = tokenType;
            Range = range;
            Value = value;
            Offset = offset;
        }
    }
}
