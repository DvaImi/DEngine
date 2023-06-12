using System.IO;
using System.Text;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using HybridCLR;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// 补充元数据流程
    /// </summary>
    public class ProcedureLoadAotMetadData : ProcedureBase
    {
        private int m_AotLength;
        private int m_LoadedAotLength;
        private bool m_LoadComplete;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_LoadComplete = false;
            LoadMetadataForAOTAssemblies();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!m_LoadComplete)
            {
                return;
            }

            ChangeState<ProcedureLoadHotfix>(procedureOwner);
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。在此放在热更层，便于后续可以热更补充元数据
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。
        /// 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        /// 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。
        /// </summary>
        private void LoadMetadataForAOTAssemblies()
        {
            Log.Debug("补充元数据...");
            GameEntry.Resource.LoadAsset(AssetUtility.GetCLRAOTAsset("AOTMetadataMainfest"), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnAOTMetadataMainfestLoadSuccessAsync)));
        }

        private void OnAOTMetadataMainfestLoadSuccessAsync(string assetName, object asset, float duration, object userData)
        {
            TextAsset result = (TextAsset)asset;
            string[] aotdll = null;
            if (result != null && result.bytes != null)
            {
                using (Stream stream = new MemoryStream(result.bytes))
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        int count = binaryReader.ReadInt32();
                        aotdll = new string[count];

                        for (int i = 0; i < count; i++)
                        {
                            aotdll[i] = AssetUtility.GetCLRAOTAsset(binaryReader.ReadString());
                        }
                        Log.Info($"AOTMetadata\n {string.Join("\n", aotdll)}\n is Ready.");
                    }
                }
            }
            m_AotLength = aotdll.Length;
            for (int i = 0; i < aotdll.Length; i++)
            {
                int index = i;
                GameEntry.Resource.LoadAsset(aotdll[index], new LoadAssetCallbacks(OndAotMetadDataLoadSuccess), index);
            }
        }

        private void OndAotMetadDataLoadSuccess(string assetName, object asset, float duration, object userData)
        {
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null || textAsset.bytes == null)
            {
                return;
            }
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
            Log.Debug($"AOTMetadata :{assetName} Load Success");
            m_LoadedAotLength++;
            if (m_LoadedAotLength == m_AotLength)
            {
                Log.Debug($"AOTMetadata Load Complete.");
                m_LoadComplete = true;
            }
        }
    }
}
