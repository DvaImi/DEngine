using DEngine;
using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game
{
    /// <summary>
    /// 使用可更新模式应用补丁包流程
    /// </summary>
    public class ProcedureApplyResourcePack : GameProcedureBase
    {
        private bool m_PatchResourcePackComplete = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("Start apply patch");
            var patchResourcePackName = procedureOwner.GetData<VarString>("PatchResourcePackName");
            string patchResourcePackPath = Utility.Path.GetRegularCombinePath(GameEntry.Resource.ReadWritePath, patchResourcePackName);
            if (!GameEntry.Resource.VerifyResourcePack(patchResourcePackPath))
            {
                Log.Warning("patch resource pack {0} is invalid", patchResourcePackPath);
                return;
            }

            Log.Info("Patch resource pack {0} valid", patchResourcePackName);
            procedureOwner.RemoveData("PatchResourcePackName");
            GameEntry.Resource.ApplyResources(patchResourcePackPath, OnApplyResourcesComplete);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!m_PatchResourcePackComplete)
            {
                return;
            }

            ChangeState<ProcedureClearCachePack>(procedureOwner);
        }

        private void OnApplyResourcesComplete(string resourcePackPath, bool result)
        {
            Log.Info("Apply resources pack {0} :{1}", resourcePackPath, result);
            if (result)
            {
                m_PatchResourcePackComplete = true;
                return;
            }

            GameEntry.BuiltinData.OpenDialog(new DialogParams
            {
                Mode = 1,
                Message = "Apply resources pack failure.",
                ConfirmText = "Quit",
                OnClickConfirm = delegate { DEngine.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            });
        }
    }
}