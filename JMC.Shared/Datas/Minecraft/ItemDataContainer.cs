namespace JMC.Shared.Datas.Minecraft
{
    internal class ItemDataContainer(IEnumerable<KeyValuePair<string, ItemData>> itemDatas) : Dictionary<string, ItemData>(itemDatas)
    {
        public bool IsExist(string t) => true;
    }
}
