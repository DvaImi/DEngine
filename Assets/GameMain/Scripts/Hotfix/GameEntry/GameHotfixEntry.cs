//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using HybridCLR;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Dvalmi.Hotfix
{
    /// <summary>
    /// 热更新入口。
    /// </summary>
    public static class GameHotfixEntry
    {
        private static int m_AOTFlag;
        private static int m_AOTLoadFlag;

        public static void Start()
        {
#if UNITY_EDITOR
            StartHotfix();
#else
            // 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
            // 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。

            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。

            m_AOTFlag = GameEntry.BuiltinData.HotfixInfo.AOTDllNames.Length;
            m_AOTLoadFlag = 0;
            for (int i = 0; i < m_AOTFlag; i++)
            {
                string dllName = GameEntry.BuiltinData.HotfixInfo.AOTDllNames[i];
                string assetName = Utility.Text.Format(GameEntry.BuiltinData.HotfixInfo.HotfixDllPath, dllName);
                GameEntry.Resource.LoadAsset(assetName, new LoadAssetCallbacks(OnLoadAOTDllSuccess, OnLoadAssetFail));
            }
#endif
        }



        private static void StartHotfix()
        {
            GameEntry.BuiltinData.DestroyDialog();
            // 重置流程组件，初始化热更新流程。
            GameEntry.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureChangeScene(),
                new ProcedureMain(),
                new ProcedureMenu(),
                new ProcedurePreload(),
            };
            procedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);
            procedureManager.StartProcedure<ProcedurePreload>();
        }

        private static void OnLoadAOTDllSuccess(string assetName, object asset, float duration, object userdata)
        {
            TextAsset dll = (TextAsset)asset;
            byte[] dllBytes = dll.bytes;
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            Log.Info($"LoadMetadataForAOTAssembly:{assetName}. ret:{err}");
            if (++m_AOTLoadFlag == m_AOTFlag)
            {
                StartHotfix();
            }
        }

        private static void OnLoadAssetFail(string assetname, LoadResourceStatus status, string errormessage, object userdata)
        {
        }
    }
}
