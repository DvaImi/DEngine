using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;
using HybridCLR;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 补充元数据流程
    /// </summary>
    public class ProcedureLoadAotMetadData : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            string[] aotFullNames = await LoadAOTMetadataVersion();
            int loadCount = await LoadMetadataForAOTAssemblies(aotFullNames);
            Log.Info("AOTMetadata Load Complete. need load count {0}，load success {1}", aotFullNames.Length, loadCount);
            ChangeState<ProcedureLoadHotUpdate>(procedureOwner);
        }

        /// <summary>
        /// 加载补充元数据列表
        /// </summary>
        /// <returns></returns>
        private async UniTask<string[]> LoadAOTMetadataVersion()
        {
            string[] aotFullNames = null;
            TextAsset result = await GameEntry.Resource.LoadAssetAsync<TextAsset>(BuiltinAssetUtility.GetCLRAOTAsset(Constant.AssetVersion.AOTMetadataVersion));
            if (result != null && result.bytes != null)
            {
                using Stream stream = new MemoryStream(result.bytes);
                using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);
                int count = binaryReader.ReadInt32();
                aotFullNames = new string[count];
                for (int i = 0; i < count; i++)
                {
                    aotFullNames[i] = BuiltinAssetUtility.GetCLRAOTAsset(binaryReader.ReadString());
                }
            }

            return aotFullNames;
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。在此放在热更层，便于后续可以热更补充元数据
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        /// 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。
        /// </summary>
        /// <param name="aotFullNames"></param>
        private async UniTask<int> LoadMetadataForAOTAssemblies(string[] aotFullNames)
        {
            Log.Info("补充元数据...");
            var textAssets = await GameEntry.Resource.LoadAssetsAsync<TextAsset>(aotFullNames);
            int metaCount = 0;
            if (textAssets == null)
            {
                return metaCount;
            }

            foreach (TextAsset textAsset in textAssets)
            {
                if (textAsset == null || textAsset.bytes == null)
                {
                    continue;
                }

                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode code = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
                if (code == LoadImageErrorCode.OK)
                {
                    metaCount++;
                    Log.Info($"AOTMetadata :{textAsset.name} Load Success");
                }
                else
                {
                    Log.Warning($"Load AOT metadata {code}");
                }
            }

            return metaCount;
        }
    }
}