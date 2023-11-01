namespace JMC.Shared.Datas.Minecraft.Blocks
{
    internal class BlockDataContainer(IEnumerable<BlockData> datas) : List<BlockData>(datas)
    {
        public bool IsExist(string name)
        {
            name = name.Split(':').Last();
            return Exists(v => v.Name.StartsWith(name));
        }
    }
}
