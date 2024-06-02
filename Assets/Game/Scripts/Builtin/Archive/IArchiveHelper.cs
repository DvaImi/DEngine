using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    /// <summary>
    /// 存档辅助器
    /// </summary>
    public interface IArchiveHelper
    {
        bool Query(string fileUri);

        bool Match(string userIdentifier);

        UniTask SaveAsync(string fileUri, byte[] bytes);

        UniTask<byte[]> LoadAsync(string fileUri);
    }
}