using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game
{
    public class ProcedureCheckResources : GameProcedureBase
    {
        private bool m_CheckResourcesComplete;
        private bool m_NeedUpdateResources;
        private int m_UpdateResourceCount;
        private long m_UpdateResourceTotalCompressedLength;
        private bool m_UseResourcePatchPack;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckResourcesComplete = false;
            m_NeedUpdateResources = false;
            m_UpdateResourceCount = 0;
            m_UpdateResourceTotalCompressedLength = 0L;
            m_UseResourcePatchPack = false;

            if (procedureOwner.HasData("UseResourcePatchPack"))
            {
                m_UseResourcePatchPack = procedureOwner.GetData<VarBoolean>("UseResourcePatchPack");
                procedureOwner.RemoveData("UseResourcePatchPack");
            }

            GameEntry.Resource.CheckResources(OnCheckResourcesComplete);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckResourcesComplete)
            {
                return;
            }

            if (m_NeedUpdateResources)
            {
                if (m_UseResourcePatchPack)
                {
                    ChangeState<ProcedureUpdateResourcePack>(procedureOwner);
                }
                else
                {
                    procedureOwner.SetData<VarInt32>("UpdateResourceCount", m_UpdateResourceCount);
                    procedureOwner.SetData<VarInt64>("UpdateResourceTotalCompressedLength", m_UpdateResourceTotalCompressedLength);
                    ChangeState<ProcedureUpdateResources>(procedureOwner);
                }
            }
            else
            {
                ProcessAssembliesProcedure(procedureOwner);
            }
        }

        private void OnCheckResourcesComplete(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalCompressedLength)
        {
            Log.Info("Check resources complete, '{0}' resources need to update, compressed length is '{1}', uncompressed length is '{2}'.", updateCount.ToString(), updateTotalCompressedLength.ToString(), updateTotalLength.ToString());
            m_CheckResourcesComplete = true;
            m_NeedUpdateResources = updateCount > 0;
            m_UpdateResourceCount = updateCount;
            m_UpdateResourceTotalCompressedLength = updateTotalCompressedLength;
        }
    }
}