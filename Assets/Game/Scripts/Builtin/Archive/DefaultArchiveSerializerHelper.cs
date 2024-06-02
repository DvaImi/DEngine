using DEngine;
using Newtonsoft.Json;

namespace Game.Archive
{
    internal sealed class DefaultArchiveSerializerHelper : IArchiveSerializerHelper
    {
        private readonly JsonSerializerSettings m_Settings;

        public DefaultArchiveSerializerHelper()
        {
            m_Settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
        }

        public byte[] Serialize<T>(T data)
        {
            return Utility.Converter.GetBytes(JsonConvert.SerializeObject(data, m_Settings));
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonConvert.DeserializeObject<T>(Utility.Converter.GetString(data), m_Settings);
        }
    }
}