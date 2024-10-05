namespace Game.Archive
{
    public interface IArchiveSerializerHelper
    {
        byte[] Serialize<T>(T data);
        
        T Deserialize<T>(byte[] data);
    }
}