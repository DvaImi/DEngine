using Game.Update.DataTable;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static class Entry
    {
        public static IDataTableProvider DataTable { get; private set; }
        public static ILubanDataProvider Luban { get; private set; }

        public static void Initialize()
        {
            Luban = GameEntry.GetModule<ILubanDataProvider>();
            DataTable = GameEntry.GetModule<IDataTableProvider>();
        }
    }
}