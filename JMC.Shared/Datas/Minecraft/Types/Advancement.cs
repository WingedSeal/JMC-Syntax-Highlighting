using System.Text.Json.Serialization;

namespace JMC.Shared.Datas.Minecraft.Types
{
    public class Advancement : DatapackJson<Advancement>
    {
        [JsonPropertyName("display")]
        public DisplayPropety? Display { get; set; }

        public class DisplayPropety
        {
            [JsonPropertyName("icon")]
            public required IconPropety Icon { get; set; }
            public class IconPropety
            {
                [JsonPropertyName("item")]
                [JsonRequired]
                public required string Item { get; set; }
                [JsonPropertyName("nbt")]
                public string? NBT { get; set; }
            }

            [JsonPropertyName("title")]
            public required object Title { get; set; }

            [JsonPropertyName("frame")]
            public FrameType? Frame { get; set; } = FrameType.Task;
            [JsonConverter(typeof(JsonStringEnumConverter<FrameType>))]
            public enum FrameType
            {
                [JsonPropertyName("challenge")]
                Challenge,
                [JsonPropertyName("goal")]
                Goal,
                [JsonPropertyName("task")]
                Task
            }

            [JsonPropertyName("background")]
            public string? Background { get; set; }
            [JsonPropertyName("description")]
            public required object Description { get; set; }
            [JsonPropertyName("show_toast")]
            public bool ShowToast { get; set; } = true;
            [JsonPropertyName("announce_to_chat")]
            public bool AnnounceToChat { get; set; } = true;
            [JsonPropertyName("hidden")]
            public bool Hidden { get; set; } = false;
        }

        [JsonPropertyName("parent")]
        public string? Parent { get; set; }
    }
}
