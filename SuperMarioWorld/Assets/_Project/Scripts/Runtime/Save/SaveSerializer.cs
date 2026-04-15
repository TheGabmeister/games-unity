using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SMW.Save
{
    public static class SaveSerializer
    {
        private static readonly JsonSerializerSettings Settings = BuildSettings();

        private static JsonSerializerSettings BuildSettings()
        {
            var s = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
            s.Converters.Add(new StringEnumConverter());
            return s;
        }

        public static string Serialize(SaveData data) => JsonConvert.SerializeObject(data, Settings);

        public static SaveData Deserialize(string json) => JsonConvert.DeserializeObject<SaveData>(json, Settings);
    }
}
