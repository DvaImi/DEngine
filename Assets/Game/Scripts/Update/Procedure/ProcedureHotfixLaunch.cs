using Cysharp.Threading.Tasks;
using DEngine.Procedure;
using DEngine.Runtime;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureHotfixLaunch : ProcedureBase
    {
        private UniTask m_InitArchiveTask;
        /// <summary>
        /// 热更新流程启动
        /// </summary>
        /// <param name="procedureOwner"></param>
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("ProcedureHotfix  Launch  ");
            m_InitArchiveTask = GameEntry.Archive.Initialize();
        }


        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (m_InitArchiveTask.Status != UniTaskStatus.Succeeded)
            {
                return;
            }
            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}