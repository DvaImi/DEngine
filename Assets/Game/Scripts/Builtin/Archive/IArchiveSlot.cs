using Cysharp.Threading.Tasks;

namespace Game.Archive
{
    public interface IArchiveSlot
    {
        /// <summary>
        /// 存档槽标识符
        /// </summary>
        string SlotId { get; }

        /// <summary>
        /// 获取目录
        /// </summary>
        /// <returns></returns>
        SlotCatalog SlotCatalog { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="archiveManager"></param>
        /// <param name="slotId"></param>
        /// <returns></returns>
        void Initialize(IArchiveManager archiveManager, string slotId);

        /// <summary>
        /// 从存档槽里同步加载存档数据
        /// </summary>
        /// <param name="identifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T LoadData<T>(string identifier) where T : IArchiveData;

        /// <summary>
        /// 往存档槽里同步保存存档数据
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        void SaveData<T>(string identifier, T data) where T : IArchiveData;

        /// <summary>
        /// 从存档槽里异步加载存档数据
        /// </summary>
        /// <param name="identifier"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask<T> LoadDataAsync<T>(string identifier) where T : IArchiveData;

        /// <summary>
        /// 往存档槽里异步保存存档数据
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask SaveDataAsync<T>(string identifier, T data) where T : IArchiveData;

        /// <summary>
        /// 删除存档
        /// </summary>
        void Delete();

        /// <summary>
        ///  备份存档
        /// </summary>
        void Backup();
    }
}