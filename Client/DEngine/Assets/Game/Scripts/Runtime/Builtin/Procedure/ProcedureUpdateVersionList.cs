using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;

namespace Game
{
    /// <summary>
    /// 使用可更新模式更新版本资源列表流程
    /// </summary>
    public class ProcedureUpdateVersionList : GameProcedureBase
    {
        private bool m_UpdateVersionComplete = false;
        private UpdateVersionListCallbacks m_UpdateVersionListCallbacks = null;

        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);

            m_UpdateVersionListCallbacks = new UpdateVersionListCallbacks(OnUpdateVersionListSuccess, OnUpdateVersionListFailure);
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_UpdateVersionComplete = false;
            int versionListLength             = procedureOwner.GetData<VarInt32>(Constant.Resource.VersionListLength);
            int versionListHashCode           = procedureOwner.GetData<VarInt32>(Constant.Resource.VersionListHashCode);
            int versionListCompressedLength   = procedureOwner.GetData<VarInt32>(Constant.Resource.VersionListCompressedLength);
            int versionListCompressedHashCode = procedureOwner.GetData<VarInt32>(Constant.Resource.VersionListCompressedHashCode);
            GameEntry.Resource.UpdateVersionList(versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode, m_UpdateVersionListCallbacks);
            procedureOwner.RemoveData(Constant.Resource.VersionListLength);
            procedureOwner.RemoveData(Constant.Resource.VersionListHashCode);
            procedureOwner.RemoveData(Constant.Resource.VersionListCompressedLength);
            procedureOwner.RemoveData(Constant.Resource.VersionListCompressedHashCode);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_UpdateVersionComplete)
            {
                return;
            }

            ChangeState<ProcedureVerifyResources>(procedureOwner);
        }

        private void OnUpdateVersionListSuccess(string downloadPath, string downloadUri)
        {
            m_UpdateVersionComplete = true;
            Log.Info("Update version list from '{0}' success.", downloadUri);
        }

        private void OnUpdateVersionListFailure(string downloadUri, string errorMessage)
        {
            Log.Warning("Update version list from '{0}' failure, error message is '{1}'.", downloadUri, errorMessage);
        }
    }
}