using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;

namespace Game
{
    public class ProcedureLoadHotUpdateEntry : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            HotfixLauncher();
        }

        private void HotfixLauncher()
        {
            GameEntry.Resource.LoadAsset(BuiltinAssetUtility.GetCLRLanuchAsset("UpdateLuncher"), new LoadAssetCallbacks(OnUpdateLuncherLoadSuccess, OnUpdateLuncherLoadFailure));
        }

        private void OnUpdateLuncherLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            Object.Instantiate((GameObject)asset);
        }

        private void OnUpdateLuncherLoadFailure(string assetname, LoadResourceStatus status, string errormessage, object userdata)
        {
            Log.Error($"Load asset {assetname}failure  errormessage is {errormessage}.");
        }
    }
}