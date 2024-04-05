﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
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
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            LoadMetadataForAOTAssemblies();
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
            Log.Info($"AOTMetadata Load Complete.");
            ChangeState<ProcedureLoadHotUpdate>(procedureOwner);
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
            Log.Info("补充元数据...");
            m_LoadedFlag.Add(BuiltinAssetUtility.GetCLRAOTAsset(Constant.AssetVersion.AOTMetadataVersion), false);
            GameEntry.Resource.LoadAsset(BuiltinAssetUtility.GetCLRAOTAsset(Constant.AssetVersion.AOTMetadataVersion), new LoadAssetCallbacks(new LoadAssetSuccessCallback(OnAOTMetadataVersionLoadSuccessAsync)));
        }

        private void OnAOTMetadataVersionLoadSuccessAsync(string assetName, object asset, float duration, object userData)
        {
            TextAsset result = (TextAsset)asset;
            if (result != null && result.bytes != null)
            {
                m_LoadedFlag[assetName] = true;
                using (Stream stream = new MemoryStream(result.bytes))
                {
                    using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        int count = binaryReader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            string aotFullName = BuiltinAssetUtility.GetCLRAOTAsset(binaryReader.ReadString());
                            Log.Info($"补充的元数据是：[{aotFullName}]");
                            m_LoadedFlag.Add(aotFullName, false);
                            GameEntry.Resource.LoadAsset(aotFullName, new LoadAssetCallbacks(OndAotMetadDataLoadSuccess));
                        }
                    }
                }
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
            m_LoadedFlag[assetName] = true;
            Log.Info($"AOTMetadata :{textAsset.name} Load Success");
        }
    }
}
