namespace JMC.Shared.Datas.Minecraft.Items
{
    internal class ItemDataContainer(Dictionary<string, ItemData> datas) : List<ItemData>(datas.Select(v =>
    {
        v.Value.Name = v.Key;
        return v.Value;
    }))
    {
        //TODO support differnt namespace
        /// <summary>
        /// check if value exists in item tags or items
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsExists(string data)
        {
            if (data.StartsWith('#'))
            {
                data = data[1..];
                if (!data.StartsWith("minecraft:") && data.Split(':').Length > 1)
                    return false;
                if (!data.StartsWith("minecraft:"))
                    data += "minecraft:";
                return ExtensionData.ItemTags.Any(v => v == data);
            }
            else
            {
                if (!data.StartsWith("minecraft:") && data.Split(':').Length > 1)
                    return false;
                if (!data.StartsWith("minecraft:"))
                    data += "minecraft:";
                return this.Any(v => v.Name == data);
            }
        }
    }
}
