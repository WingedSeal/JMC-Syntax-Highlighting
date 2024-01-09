using System.Text.Json;

namespace JMC.Shared.Datas.Minecraft.Types
{
    public interface IDatapackJson<in T>
    {
        public static abstract JsonException? Validate(string datapackJson);
    }
}
