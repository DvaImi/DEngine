using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    /// <summary>
    /// 存档辅助器
    /// </summary>
    public interface IArchiveHelper
    {
        /// <summary>
        /// 存档路径
        /// </summary>
        string ArchiveUrl { get; }

        /// <summary>
        /// 创建存储单元
        /// </summary>
        /// <returns></returns>
        IArchiveSlot CreateArchiveSlot();

        /// <summary>
        /// 设置存档路径
        /// </summary>
        /// <param name="archiveUrl"></param>
        void SetArchiveUrl(string archiveUrl);

        /// <summary>
        /// 设置加密解密辅助器
        /// </summary>
        void SetEncryptor(IEncryptorHelper encryptorHelper);

        /// <summary>
        /// 同步加载数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        byte[] Load(string filePath);

        /// <summary>
        /// 同步保存数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        void Save(string filePath, byte[] data);

        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        UniTask<byte[]> LoadAsync(string filePath);

        /// <summary>
        /// 异步保存数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        UniTask SaveAsync(string filePath, byte[] data);

        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param name="filePath"></param>
        void Delete(string filePath);

        /// <summary>
        /// 备份存档
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        void Backup(string sourcePath, string destinationPath);
    }
}