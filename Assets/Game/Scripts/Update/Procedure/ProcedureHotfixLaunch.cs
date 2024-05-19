using DEngine.Procedure;
using DEngine.Runtime;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureHotfixLaunch : ProcedureBase
    {
        private bool m_InitArchiveComplete;
        private int m_SlotCount;

        /// <summary>
        /// 热更新流程启动
        /// </summary>
        /// <param name="procedureOwner"></param>
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            m_InitArchiveComplete = false;
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("ProcedureHotfix  Launch  ");
            GameEntry.Archive.Initialize(OnInitArchiveCompleteCallback);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (!m_InitArchiveComplete)
            {
                return;
            }

            procedureOwner.SetData<VarInt32>("ArchiveSlotCount", m_SlotCount);
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        private void OnInitArchiveCompleteCallback(int version, int slotCount)
        {
            Log.Info("Init archive complete, current version is  '{0}' slot count is  '{1}'.", version, slotCount);
            m_SlotCount = slotCount;
            m_InitArchiveComplete = true;
        }
    }
}