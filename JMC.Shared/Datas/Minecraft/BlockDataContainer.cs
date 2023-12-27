namespace JMC.Shared.Datas.Minecraft
{
    internal class BlockDataContainer(IEnumerable<KeyValuePair<string, BlockData>> blockDatas) : Dictionary<string, BlockData>(blockDatas)
    {
        public bool IsExist(string t) => true;
    }
}
