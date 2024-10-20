using System;
using Cysharp.Threading.Tasks;
using DEngine.Procedure;
using DEngine.Runtime;
using UnityEngine;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update.Procedure
{
    public class ProcedurePreload : ProcedureBase
    {
        private UniTask m_PreloadTask;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_PreloadTask = PreloadResourcesTask();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            switch (m_PreloadTask.Status)
            {
                case UniTaskStatus.Pending:
                    return;
                case UniTaskStatus.Succeeded:
                    ChangeState<ProcedureProcessingPreload>(procedureOwner);
                    break;
                case UniTaskStatus.Faulted:
                    try
                    {
                        //触发异常机制
                        m_PreloadTask.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Preload task failed with exception: {ex}");
                    }

                    break;
                case UniTaskStatus.Canceled:
                    break;
                default:
                    Log.Error("Unknown Status");
                    break;
            }
        }

        private static async UniTask PreloadResourcesTask()
        {
            try
            {
                await UniTask.WhenAll(PreloadLocalization(), PreloadShaderVariants());
            }
            catch (Exception e)
            {
                Log.Error("Preload resources exception : {0}.", e.Message);
            }

            Log.Info("Preload resources complete.");
        }


        private static async UniTask PreloadLocalization()
        {
            await GameEntry.Localization.LoadDictionaryAsync(UpdateAssetUtility.GetDictionaryAsset(GameEntry.Localization.Language.ToString(), true));
        }

        private static async UniTask PreloadShaderVariants()
        {
            ShaderVariantCollection shaderVariantCollection = await GameEntry.Resource.LoadAssetAsync<ShaderVariantCollection>(UpdateAssetUtility.GetShaderVariantsAsset("GameShaderVariants"));
            if (!shaderVariantCollection || shaderVariantCollection.isWarmedUp)
            {
                return;
            }

            shaderVariantCollection.WarmUp();
            Log.Info("Game ShaderVariants has WarmUp.");
        }
    }
}