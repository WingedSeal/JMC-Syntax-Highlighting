using Newtonsoft.Json;

namespace JMC.Shared.Datas.Minecraft.Items
{
    internal class ItemData
    {
        public string Name { get; set; } = string.Empty;
        [JsonProperty("id")]
        [JsonRequired]
        public int ID { get; set; } = -1;
    }
}
