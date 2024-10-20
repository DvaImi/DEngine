using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    // 存档管理接口
    public interface IArchiveManager : IGameModule
    {
        /// <summary>
        /// 使用加密
        /// </summary>
        bool UserEncryptor { get; }

        /// <summary>
        /// 获取当前存档栏
        /// </summary>
        ArchiveSlot CurrentSlot { get; }

        /// <summary>
        /// 设置存档辅助器
        /// </summary>
        /// <param name="archiveHelper">存档辅助器</param>
        void SetArchiveHelper(IArchiveHelper archiveHelper);

        /// <summary>
        ///  设置序列化辅助器
        /// </summary>
        /// <param name="serializerHelper">序列化辅助器</param>
        void SetArchiveSerializerHelper(IArchiveSerializerHelper serializerHelper);

        /// <summary>
        ///  设置加密解密辅助器
        /// </summary>
        /// <param name="encryptorHelper">加密解密辅助器</param>
        void SetEncryptorHelper(IEncryptorHelper encryptorHelper);

        /// <summary>
        /// 初始化存档系统
        /// </summary>
        /// <param name="archiveUri"></param>
        /// <param name="maxSlotCount"></param>
        /// <param name="userIdentifier"></param>
        /// <returns></returns>
        UniTask Initialize(string archiveUri, int maxSlotCount, string userIdentifier);

        /// <summary>
        /// 获取存档栏
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ArchiveSlot GetArchiveSlot(int index);

        /// <summary>
        /// 获取所有存档栏
        /// </summary>
        /// <returns></returns>
        ArchiveSlot[] GetArchiveSlots();

        /// <summary>
        /// 选择存档栏
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        void SelectSlot(int index);

        /// <summary>
        /// 设置存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        void SetData<T>(T data) where T : IArchiveData;

        /// <summary>
        /// 是否存在存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool HasData<T>(string uniqueId) where T : IArchiveData;

        /// <summary>
        /// 通过指定标识符获取存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId">要获取的存档数据标识符</param>
        /// <returns></returns>
        T GetData<T>(string uniqueId) where T : IArchiveData;

        /// <summary>
        /// 通过指定标识符获取存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <param name="defaultData">当指定的存档项不存在时，返回此默认值</param>
        /// <returns></returns>
        T GetData<T>(string uniqueId, T defaultData) where T : IArchiveData;

        /// <summary>
        /// 获取所有的存档数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T[] GetDatas<T>() where T : IArchiveData;

        /// <summary>
        /// 保存存档
        /// </summary>
        UniTask Save();

        /// <summary>
        /// 加载存档
        /// </summary>
        UniTask Load();

        /// <summary>
        /// 保存当前<see cref="CurrentSlot"/>元数据
        /// </summary>
        /// <returns></returns>
        UniTask SaveSlotMeta();

        /// <summary>
        /// 保存指定元数据
        /// </summary>
        /// <param name="archiveSlot"></param>
        /// <returns></returns>
        UniTask SaveSlotMeta(ArchiveSlot archiveSlot);
    }
}