using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace JMC.Extension.Server
{
    internal static class ExtensionSelector
    {
        public static readonly DocumentSelector JMC = new(
            new DocumentFilter()
            {
                Pattern = "**/*.jmc",
                Language = "jmc"
            }
        );

        public static readonly DocumentSelector HJMC = new(
            new DocumentFilter()
            {
                Pattern = "**/*.hjmc",
                Language = "hjmc"
            }
        );
    }
}
