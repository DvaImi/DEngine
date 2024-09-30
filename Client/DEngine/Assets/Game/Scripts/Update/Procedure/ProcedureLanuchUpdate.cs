using DEngine.Procedure;
using DEngine.Runtime;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update
{
    public class ProcedureLanuchUpdate : ProcedureBase
    {
        protected override async void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await Entry.Initialize();
            Log.Info("ProcedureHotfix  Launch  ");
            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}