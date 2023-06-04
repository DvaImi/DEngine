// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 12:44:18
// 版 本：1.0
// ========================================================
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using HybridCLR;
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
            HotfixLauncher().Forget();
#else
            LoadHotfixDll().Forget();
#endif
        }

        private async UniTask HotfixLauncher()
        {
            var launch = await GameEntry.Resource.LoadAssetAsync<GameObject>(AssetUtility.GetAddress("GameHotfixEntry"));
            Object.Instantiate(launch);
        }

        private async UniTask LoadHotfixDll()
        {
            if (m_HasLoadHotfixDll)
            {
                Log.Debug("已经加载过热更新dll ，暂时无法重复加载");
                await HotfixLauncher();
                return;
            }
            var dll = await GameEntry.Resource.LoadAssetAsync<TextAsset>(AssetUtility.GetAddress(GameEntry.BuiltinData.HotfixInfo.HotfixDllNameMain));
            Assembly hotfixAssembly = Assembly.Load(dll.bytes);
            if (hotfixAssembly == null)
            {
                Log.Fatal(Utility.Text.Format("Load hotfix dll {0} is Fail", GameEntry.BuiltinData.HotfixInfo.HotfixDllNameMain));
                return;
            }
            Log.Info("Load hotfix dll OK.");
            m_HasLoadHotfixDll = true;

            await LoadMetadataForAOTAssemblies();
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        /// 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。
        /// </summary>
        private async UniTask LoadMetadataForAOTAssemblies()
        {

            string[] aotdll = GameEntry.BuiltinData.HotfixInfo.AOTDllNames;
            if (aotdll == null)
            {
                Log.Fatal("AOTAssemblies is invalid.");
                return;
            }

            var aotdlls = await GameEntry.Resource.LoadAssetsAsync<TextAsset>(AssetUtility.GetAddress(aotdll));
            if (aotdlls == null)
            {
                Log.Fatal("AOTAssemblies Load fail.");
                return;
            }

            for (int i = 0; i < aotdlls.Length; i++)
            {
                TextAsset dll = aotdlls[i];
                byte[] dllBytes = dll.bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
                Log.Info($"LoadMetadataForAOTAssembly:{dll.name}. ret:{err}");
            }

            await HotfixLauncher();
        }
    }
}
