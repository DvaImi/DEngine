// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 12:44:18
// 版 本：1.0
// ========================================================
using System.Reflection;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace Game
{
    public class ProcedureLoadHotfix : ProcedureBase
    {
        private static bool m_HasLoadHotfixDll;
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
            if (m_HasLoadHotfixDll)
            {
                Log.Debug("已经加载过热更新dll ，暂时无法重复加载");
                HotfixLauncher();
                return;
            }
            GameEntry.Resource.LoadAsset(GameEntry.BuiltinData.HotfixInfo.GetHotfixMainDllFullName(), new LoadAssetCallbacks(OnLoadHotfixDllSuccess, OnLoadHotfixDllFailurel));
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
                Log.Fatal(Utility.Text.Format("Load hotfix dll {0} is Fail", assetName));
                return;
            }
            Log.Info("Load hotfix dll OK.");
            m_HasLoadHotfixDll = true;
            HotfixLauncher();
        }

        private void OnLoadGameHotfixAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            GameObject game = Object.Instantiate((GameObject)asset);
            game.name = "[GameHotfixEntry]";
        }

        private void OnLoadGameHotfixAssetFailure(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load  game hotfix entry failed. " + errorMessage);
        }
    }
}
