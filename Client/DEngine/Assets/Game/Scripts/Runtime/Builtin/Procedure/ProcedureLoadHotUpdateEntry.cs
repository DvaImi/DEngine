using Cysharp.Threading.Tasks;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public class ProcedureLoadHotUpdateEntry : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            HotfixLauncher().Forget();
        }

        private static async UniTask HotfixLauncher()
        {
            var launcher = await GameEntry.Resource.LoadAssetAsync<GameObject>(BuiltinAssetUtility.GetUpdateLauncherAsset());
            if (!launcher)
            {
                Log.Error("Load asset launcher failure.");
                return;
            }

            GameEntry.Resource.UnloadAsset(launcher);
            Object.Instantiate(launcher);
        }
    }
}