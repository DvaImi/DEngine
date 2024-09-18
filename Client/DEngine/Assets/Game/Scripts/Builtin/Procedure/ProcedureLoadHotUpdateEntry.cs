using Cysharp.Threading.Tasks;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public class ProcedureLoadHotUpdateEntry : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            HotfixLauncher().Forget();
        }

        private async UniTaskVoid HotfixLauncher()
        {
            GameObject luncher = await GameEntry.Resource.LoadAssetAsync<GameObject>(BuiltinAssetUtility.GetCLRLanuchAsset("UpdateLuncher"));
            if (luncher == null)
            {
                Log.Error("Load asset luncher failure.");
                return;
            }

            Object.Instantiate(luncher);
        }
    }
}