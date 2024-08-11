using DEngine;

namespace Game.Archive
{
    internal sealed class DefaultArchiveSerializerHelper : IArchiveSerializerHelper
    {
        public byte[] Serialize<T>(T data)
        {
            return Utility.Converter.GetBytes(Utility.Json.ToJson(data));
        }

        public T Deserialize<T>(byte[] data)
        {
            return Utility.Json.ToObject<T>(Utility.Converter.GetString(data));
        }
    }
}