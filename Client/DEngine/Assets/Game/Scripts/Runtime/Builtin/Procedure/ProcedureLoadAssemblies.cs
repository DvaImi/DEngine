using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using Game.FileSystem;
using HybridCLR;

namespace Game
{
    /// <summary>
    /// 加载程序集流程
    /// </summary>
    public class ProcedureLoadAssemblies : ProcedureBase
    {
        private readonly List<UniTask<Assembly>> m_PatchTask = new();

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            LoadMetadataForAOTAssembly();
            LoadUpdateAssemblies(procedureOwner).Forget();
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        /// 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。
        /// </summary>
        private void LoadMetadataForAOTAssembly()
        {
            FileSystemDataVersion aotVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(BuiltinAssetUtility.GetAssembliesAsset("aotVersion")));
            if (aotVersion == null)
            {
                Log.Warning("aotVersion is invalid");
                return;
            }

            foreach (var fileInfo in aotVersion.FileInfos)
            {
                byte[] bytes = GameEntry.Resource.LoadBinarySegmentFromFileSystem(aotVersion.FileSystem, (int)fileInfo.Value.Offset, fileInfo.Value.Length);
                LoadImageErrorCode code = RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet);
                if (code == LoadImageErrorCode.OK)
                {
                    Log.Info($"AOTMetadata :{fileInfo.Key} Load Success");
                }
                else
                {
                    Log.Warning($"Load AOT metadata {code}");
                }
            }
        }

        /// <summary>
        /// 加载热更程序集
        /// </summary>
        /// <param name="procedureOwner"></param>
        private async UniTask LoadUpdateAssemblies(IFsm<IProcedureManager> procedureOwner)
        {
            var patchVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(BuiltinAssetUtility.GetAssembliesAsset("patchVersion")));
            if (patchVersion == null)
            {
                Log.Warning("patchVersion is invalid");
                return;
            }

            m_PatchTask.Clear();
            foreach (var bytes in patchVersion.FileInfos.Select(fileInfo => GameEntry.Resource.LoadBinarySegmentFromFileSystem(patchVersion.FileSystem, (int)fileInfo.Value.Offset, fileInfo.Value.Length)))
            {
                m_PatchTask.Add(UniTask.RunOnThreadPool(() => Assembly.Load(bytes)));
            }

            await UniTask.WhenAll(m_PatchTask);
            await UniTask.NextFrame();
            Log.Info("HotUpdateAssemblies Load Complete.");
            ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);
        }
    }
}