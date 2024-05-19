using DEngine;
using Newtonsoft.Json;

namespace Game.Archive
{
    internal sealed class DefaultArchiveSerializerHelper : IArchiveSerializerHelper
    {
        public byte[] Serialize<T>(T data)
        {
            var jsonString = JsonConvert.SerializeObject(data);
            return Utility.Converter.GetBytes(jsonString);
        }

        public T Deserialize<T>(byte[] data)
        {
            var jsonString = Utility.Converter.GetString(data);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}