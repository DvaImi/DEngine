using DEngine.Fsm;
using DEngine.Procedure;

namespace Game
{
    public abstract class GameProcedureBase : ProcedureBase
    {
        /// <summary>
        ///  处理程序集流程方法
        /// </summary>
        /// <param name="procedureOwner"></param>
        protected void ProcessAssembliesProcedure(IFsm<IProcedureManager> procedureOwner)
        {
#if ENABLE_HYBRIDCLR && !UNITY_EDITOR
            ChangeState<ProcedureLoadAssemblies>(procedureOwner);
#else
            ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);
#endif
        }
    }
}