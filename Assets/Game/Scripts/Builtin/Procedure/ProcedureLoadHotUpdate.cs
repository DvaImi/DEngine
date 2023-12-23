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

namespace Game
{
    
    public class ProcedureLoadHotUpdate : ProcedureBase
    {
        private static bool m_HasLoadHotUpdateAssemblies;
        private int m_HotUpdateAssembliesLength;
        private int m_LoadedHotUpdateAssembly;
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_HotUpdateAssembliesLength = 0;
            LoadUpdateMainfest();
        }

        private void LoadUpdateMainfest()
        {
            GameEntry.Resource.LoadAsset(AssetUtility.GetCLRUpdateAsset("HotUpdateAssembliesMainfest"), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnUpdateMainfestLoadSuccess)));
        }

        private void OnUpdateMainfestLoadSuccess(string assetName, object asset, float duration, object userData)
        {
#if !UNITY_EDITOR
            HotfixLauncher();
#else
            if (asset is TextAsset updateMainfest)
            {
                using (Stream stream = new MemoryStream(updateMainfest.bytes))
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        int count = binaryReader.ReadInt32();
                        m_HotUpdateAssembliesLength = count;
                        for (int i = 0; i < count; i++)
                        {
                            string aotFullName = AssetUtility.GetCLRUpdateAsset(binaryReader.ReadString());
                            LoadHotfixDll(aotFullName);
                        }
                    }
                }
            }
#endif
        }

        private void HotfixLauncher()
        {
            GameEntry.Resource.LoadAsset(AssetUtility.GetCLRLanuchAsset("UpdateLuncher"), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnUpdateLuncherLoadSuccess)));
        }

        private void OnUpdateLuncherLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            Object.Instantiate((GameObject)asset);
        }

        private void LoadHotfixDll(string hotUpdateAssemblies)
        {
            if (m_HasLoadHotUpdateAssemblies)
            {
                Log.Info("已经加载过热更新dll ，暂时无法重复加载");
                HotfixLauncher();
                return;
            }
            GameEntry.Resource.LoadAsset(hotUpdateAssemblies, new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnHotUpdateDllLoadSuccess)));
        }

        private void OnHotUpdateDllLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            if (asset is TextAsset hotUpdate)
            {
                Assembly hotfixAssembly = Assembly.Load(hotUpdate.bytes);
                if (hotfixAssembly == null)
                {
                    Log.Fatal(Utility.Text.Format("Load hotfix dll {0} is Fail", assetName));
                    return;
                }

                m_LoadedHotUpdateAssembly++;
                if (m_LoadedHotUpdateAssembly == m_HotUpdateAssembliesLength)
                {
                    Log.Info("{0} Load Success.", assetName);
                    m_HasLoadHotUpdateAssemblies = true;
                    HotfixLauncher();
                }
            }
        }
    }
}
