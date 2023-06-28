// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 12:44:18
// 版 本：1.0
// ========================================================
using System.IO;
using System.Reflection;
using System.Text;
using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public class ProcedureLoadHotfix : ProcedureBase
    {
        private static bool m_HasLoadHotfixDll;

        /// <summary>
        /// 主热更程序集
        /// </summary>
        private string m_HotUpdateDllNameMain;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            LoadUpdateMainfest();
        }

        private void LoadUpdateMainfest()
        {
            GameEntry.Resource.LoadAsset(AssetUtility.GetCLRUpdateAsset("UpdataDllMainfest"), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnUpdateMainfestLoadSuccess)));
        }

        private void OnUpdateMainfestLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            if (asset is TextAsset updateMainfest)
            {
                using (Stream stream = new MemoryStream(updateMainfest.bytes))
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        m_HotUpdateDllNameMain = binaryReader.ReadString();
                        Log.Info("Hybridclr is Ready.");
#if UNITY_EDITOR
                        HotfixLauncher();
#else
                        LoadHotfixDll();
#endif
                    }
                }
            }
        }

        private void HotfixLauncher()
        {
            GameEntry.Resource.LoadAsset(AssetUtility.GetCLRLanuchAsset("UpdateLuncher"), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnUpdateLuncherLoadSuccess)));
        }

        private void OnUpdateLuncherLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            Object.Instantiate((GameObject)asset);
        }

        private void LoadHotfixDll()
        {
            if (m_HasLoadHotfixDll)
            {
                Log.Info("已经加载过热更新dll ，暂时无法重复加载");
                HotfixLauncher();
                return;
            }
            GameEntry.Resource.LoadAsset(AssetUtility.GetCLRUpdateAsset(m_HotUpdateDllNameMain), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnHotUpdateDllLoadSuccess)));
        }

        private void OnHotUpdateDllLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            if (asset is TextAsset hotUpdate)
            {
                Assembly hotfixAssembly = Assembly.Load(hotUpdate.bytes);
                if (hotfixAssembly == null)
                {
                    Log.Fatal(Utility.Text.Format("Load hotfix dll {0} is Fail", m_HotUpdateDllNameMain));
                    return;
                }
                Log.Info("load hotfix dll OK.");
                HotfixLauncher();
                m_HasLoadHotfixDll = true;
            }
        }
    }
}
