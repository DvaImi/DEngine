using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game.Update.Procedure
{
    public class ProcedureEnterGame : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("==============EnterGame=================");
        }
    }
}