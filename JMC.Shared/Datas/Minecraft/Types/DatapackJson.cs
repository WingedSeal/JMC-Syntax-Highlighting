using System.Text.Json;

namespace JMC.Shared.Datas.Minecraft.Types
{
    public abstract class DatapackJson<T> : IDatapackJson<T>
    {
        public static JsonException? Validate(string datapackJson)
        {
            try
            {
                JsonSerializer.Deserialize<T>(datapackJson);
            }
            catch (JsonException ex)
            {
                return ex;
            }
            return null;
        }
    }
}
