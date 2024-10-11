using Cysharp.Threading.Tasks;
using Game.LubanTable;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static class Entry
    {
        public static ILubanModule Luban { get; private set; }

        public static async UniTask Initialize()
        {
            Luban = GameEntry.GetModule<ILubanModule>().Initialize();
            GameEntry.Network.Initialize(true, 5, AssemblyUtility.GetAssemblies());
            await UniTask.CompletedTask;
        }
    }
}