using DEngine.Procedure;
using DEngine.Runtime;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update.Procedure
{
    public class ProcedureLanuchUpdate : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Update.Entry.Initialize();
            GameEntry.Network.Initialize(true, 5, AssemblyUtility.GetAssemblies());
            Log.Info("ProcedureHotfix  Launch  ");
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}