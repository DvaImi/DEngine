using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Runtime;

namespace Game.Update.Procedure
{
    public class ProcedureEnterUpdateDomain : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            procedureOwner.SetData<VarInt32>(UpdateConstant.ProceureConstant.NextSceneId, (int)SceneId.GAMESCENE);
            procedureOwner.SetData<VarProcedure>(UpdateConstant.ProceureConstant.NextProcedure, typeof(ProcedureEnterGame));
            GameEntry.UI.SetGlobalUIClickSound(1);
            UpdateDomain.Scene.Publish(new ProcessingPreloadEventType());
        }


        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }
    }
}