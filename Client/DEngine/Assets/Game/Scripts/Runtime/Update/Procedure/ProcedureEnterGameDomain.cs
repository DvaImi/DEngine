using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game.Update.Procedure
{
    public class ProcedureEnterGameDomain : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            procedureOwner.SetData<VarInt32>(UpdateConstant.ProcedureConstant.NextSceneId, (int)SceneId.GAMESCENE);
            procedureOwner.SetData<VarProcedure>(UpdateConstant.ProcedureConstant.NextProcedure, typeof(ProcedureEnterGame));
            GameDomain.Input.SetGlobalUIClickSound((int)UISoundId.Basics);
            GameDomain.Scene.Publish(new ProcessingPreloadEventType());
        }


        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }
    }
}