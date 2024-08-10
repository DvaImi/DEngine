namespace Game.Archive
{
    internal sealed class DefaultArchiveSerializerHelper : IArchiveSerializerHelper
    {
        public byte[] Serialize<T>(T data)
        {
            return MemoryPack.MemoryPackSerializer.Serialize<T>(data);
        }

        public T Deserialize<T>(byte[] data)
        {
            return MemoryPack.MemoryPackSerializer.Deserialize<T>(data);
        }
    }
}