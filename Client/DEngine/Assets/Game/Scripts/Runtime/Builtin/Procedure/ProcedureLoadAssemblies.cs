using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using DEngine;
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
    public class ProcedureLoadAssemblies : GameProcedureBase
    {
        private static bool s_MetadataForAOTLoaded;
        private static bool s_UpdateAssembliesLoaded;
        private UniTask m_LoadAssembliesTask;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            if (s_MetadataForAOTLoaded && s_UpdateAssembliesLoaded)
            {
                return;
            }

            LoadMetadataForAOTAssembly();
            m_LoadAssembliesTask = LoadUpdateAssemblies();
        }


        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (s_MetadataForAOTLoaded && s_UpdateAssembliesLoaded)
            {
                ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);
                return;
            }

            switch (m_LoadAssembliesTask.Status)
            {
                case UniTaskStatus.Pending:
                    return;
                case UniTaskStatus.Succeeded:
                    s_UpdateAssembliesLoaded = true;
                    break;
                case UniTaskStatus.Faulted:
                    try
                    {
                        m_LoadAssembliesTask.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Task failed with exception: {ex}");
                    }

                    break;
                case UniTaskStatus.Canceled:
                    break;
                default:
                    Log.Error("Unknown Status");
                    break;
            }
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        /// 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。
        /// </summary>
        private static void LoadMetadataForAOTAssembly()
        {
            if (s_MetadataForAOTLoaded)
            {
                return;
            }

            FileSystemDataVersion aotVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(BuiltinAssetUtility.GetAssembliesAsset("aotVersion")));
            if (aotVersion == null)
            {
                Log.Warning("aotVersion is invalid");
                return;
            }

            foreach (var fileInfo in aotVersion.FileInfos)
            {
                byte[] bytes = GameEntry.Resource.LoadBinarySegmentFromFileSystem(aotVersion.FileSystem, (int)fileInfo.Value.Offset, fileInfo.Value.Length);
                byte[] hashBytes = Utility.Converter.GetBytes(fileInfo.Value.HashCode);
                bytes = Utility.Encryption.GetXorBytes(bytes, hashBytes);
                var code = RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet);
                if (code == LoadImageErrorCode.OK)
                {
                    Log.Info($"AOTMetadata :{fileInfo.Key} Load Success");
                }
                else
                {
                    Log.Warning($"Load AOT metadata {code}");
                }
            }

            s_MetadataForAOTLoaded = true;
        }

        /// <summary>
        /// 加载热更程序集
        /// </summary>
        private static async UniTask LoadUpdateAssemblies()
        {
            if (s_UpdateAssembliesLoaded)
            {
                return;
            }

            var patchVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(BuiltinAssetUtility.GetAssembliesAsset("patchVersion")));
            if (patchVersion == null)
            {
                Log.Warning("patchVersion is invalid");
                return;
            }

            using var parallel = UniTaskParallel.Creat();
            foreach (var fileInfo in patchVersion.FileInfos)
            {
                byte[] data = GameEntry.Resource.LoadBinarySegmentFromFileSystem(patchVersion.FileSystem, (int)fileInfo.Value.Offset, fileInfo.Value.Length);
                byte[] hashBytes = Utility.Converter.GetBytes(fileInfo.Value.HashCode);
                data = Utility.Encryption.GetXorBytes(data, hashBytes);
                parallel.Push(UniTask.RunOnThreadPool(LoadAssembly));
                continue;

                void LoadAssembly()
                {
                    Assembly.Load(data);
                }
            }

            await parallel.WhenAll();
            Log.Info("HotUpdateAssemblies Load Complete.");
        }
    }
}