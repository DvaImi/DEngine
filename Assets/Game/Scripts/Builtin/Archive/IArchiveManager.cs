using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    /// <summary>
    /// 存档管理器
    /// </summary>
    public interface IArchiveManager
    {
        /// <summary>
        /// 存档路径
        /// </summary>
        string ArchiveUrl { get; }

        /// <summary>
        /// 最大存档数量
        /// </summary>
        int MaxSlotCount { get; set; }

        /// <summary>
        /// 获取或者设置是否自动保存
        /// </summary>
        bool EnableAutoSave { get; set; }

        /// <summary>
        /// 获取或者设置自动更新到存储介质间隔
        /// </summary>
        float AutoSaveInterval { get; set; }

        /// <summary>
        /// 获取存档辅助器
        /// </summary>
        IArchiveHelper ArchiveHelper { get; }

        /// <summary>
        /// 获取存档序列化辅助器
        /// </summary>
        IArchiveSerializerHelper ArchiveSerializerHelper { get; }

        /// <summary>
        /// 设置存档路径
        /// </summary>
        /// <param name="archiveUrl"></param>
        void SetArchiveUrl(string archiveUrl);

        /// <summary>
        /// 设置存档辅助器
        /// </summary>
        /// <param name="archiveHelper"></param>
        void SetArchiveHelper(IArchiveHelper archiveHelper);

        /// <summary>
        /// 设置序列化辅助器
        /// </summary>
        /// <param name="serializerHelper"></param>
        void SetSerializer(IArchiveSerializerHelper serializerHelper);

        /// <summary>
        /// 设置加密解密辅助器
        /// </summary>
        /// <param name="encryptorHelper"></param>
        void SetEncryptor(IEncryptorHelper encryptorHelper);

        /// <summary>
        /// 初始化存档系统
        /// </summary>
        /// <param name="completeCallback"></param>
        void Initialize(InitArchiveCompleteCallback completeCallback);

        /// <summary>
        /// 
        /// </summary>
        bool AddArchiveSlot(string slotName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="identifier"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        void SaveData<T>(string slotName, string identifier, T data) where T : IArchiveData;

        /// <summary>
        /// 异步保存存档数据
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="identifier"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask SaveDataAsync<T>(string slotName, string identifier, T data) where T : IArchiveData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="identifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T LoadData<T>(string slotName, string identifier) where T : IArchiveData;

        /// <summary>
        /// 异步加载存档数据
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="identifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask<T> LoadDataAsync<T>(string slotName, string identifier) where T : IArchiveData;

        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param name="slotName"></param>
        void Delete(string slotName);

        /// <summary>
        ///  备份存档
        /// </summary>
        /// <param name="slotName"></param>
        void Backup(string slotName);
    }
}