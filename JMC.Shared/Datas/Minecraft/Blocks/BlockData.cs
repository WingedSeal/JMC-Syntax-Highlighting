using Newtonsoft.Json;

namespace JMC.Shared.Datas.Minecraft.Blocks
{
    internal class BlockData
    {
        [JsonProperty("id")]
        [JsonRequired]
        public int ID { get; set; } = -1;
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; } = string.Empty;
    }
}
