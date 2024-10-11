using DEngine.Fsm;
using DEngine.Procedure;

namespace Game.Update.Procedure
{
    public class ProcedureGame : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
#if FANTASY_UNITY
            GameEntry.Network.Connect("127.0.0.1:20000");
#endif
        }
    }
}