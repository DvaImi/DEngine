// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 12:44:18
// 版 本：1.0
// ========================================================
using System.Reflection;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Dvalmi
{
    public class ProcedureLoadHotfix : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

#if UNITY_EDITOR
            HotfixLauncher();
#else
            LoadHotfixDll();
#endif
        }

        private void HotfixLauncher()
        {
            GameEntry.Resource.LoadAsset(GameEntry.BuiltinData.HotfixInfo.HotfixLauncher, new LoadAssetCallbacks(OnLoadGameHotfixAssetSuccess, OnLoadGameHotfixAssetFailure));
        }

        private void LoadHotfixDll()
        {
            GameEntry.Resource.LoadAsset(GameEntry.BuiltinData.HotfixInfo.HotfixMainDllFullName, new LoadAssetCallbacks(OnLoadHotfixDllSuccess, OnLoadHotfixDllFailurel));
        }

        private void OnLoadHotfixDllFailurel(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load  dll failed. " + errorMessage);
        }

        private void OnLoadHotfixDllSuccess(string assetName, object asset, float duration, object userData)
        {
            TextAsset dll = (TextAsset)asset;
            Assembly hotfixAssembly = Assembly.Load(dll.bytes);
            if (hotfixAssembly == null)
            {
                Log.Fatal($"Load hotfix dll {assetName} is Fail");
                return;
            }
            Log.Info("Load hotfix dll OK.");
            HotfixLauncher();
        }

        private void OnLoadGameHotfixAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            GameObject game = UnityEngine.Object.Instantiate((GameObject)asset);
            game.name = "[GameHotfixEntry]";
        }

        private void OnLoadGameHotfixAssetFailure(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load  game hotfixentry failed. " + errorMessage);
        }
    }
}
