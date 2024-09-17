﻿// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 12:44:18
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
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
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            LoadUpdateMainfest();
        }
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            foreach (KeyValuePair<string, bool> loadedFlag in m_LoadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    return;
                }
            }

            ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);

        }
        private void LoadUpdateMainfest()
        {
            m_LoadedFlag.Add(BuiltinAssetUtility.GetCLRUpdateAsset(Constant.AssetVersion.HotUpdateAssembliesVersion), false);
            GameEntry.Resource.LoadAsset(BuiltinAssetUtility.GetCLRUpdateAsset(Constant.AssetVersion.HotUpdateAssembliesVersion), new LoadAssetCallbacks(OnUpdateAssembliesVersionLoadSuccess));
        }

        private void OnUpdateAssembliesVersionLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            if (asset is TextAsset updateMainfest)
            {
                using (Stream stream = new MemoryStream(updateMainfest.bytes))
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        int count = binaryReader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            string aotFullName = BuiltinAssetUtility.GetCLRUpdateAsset(binaryReader.ReadString());
                            m_LoadedFlag.Add(aotFullName, false);
                            LoadHotfixDll(aotFullName);
                        }
                    }
                }
            }
            m_LoadedFlag[assetName] = true;
        }

        private void LoadHotfixDll(string hotUpdateAssemblies)
        {
            GameEntry.Resource.LoadAsset(hotUpdateAssemblies, new LoadAssetCallbacks(OnHotUpdateDllLoadSuccess));
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

                Log.Info("{0} Load Success.", assetName);
                m_LoadedFlag[assetName] = true;
            }
        }
    }
}
