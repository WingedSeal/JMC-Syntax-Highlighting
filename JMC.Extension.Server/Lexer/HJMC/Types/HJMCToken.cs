using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server.Lexer.HJMC.Types
{
    public class HJMCToken
    {
        public HJMCTokenType Type { get; set; }
#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null ?。請考慮宣告為可為 Null。
        public List<string> Values { get;set; }
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null ?。請考慮宣告為可為 Null。
    }
}
