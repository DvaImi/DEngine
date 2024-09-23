using Cysharp.Threading.Tasks;
using Game.LubanTable;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static class UpdateEntry
    {
        public static ILubanModule Luban { get; private set; }

        public static async UniTask Initialize()
        {
            Luban = GameEntry.GetModule<ILubanModule>().Initialize();
            GameEntry.Network.Initialize(AssemblyUtility.GetAssemblies());
            await GameEntry.Archive.Initialize();
        }
    }
}