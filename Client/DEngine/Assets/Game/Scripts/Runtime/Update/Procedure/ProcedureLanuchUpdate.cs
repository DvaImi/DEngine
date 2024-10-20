using System;
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
            Entry.Initialize();
            Entry.Network.Initialize(true, 5, AssemblyUtility.GetAssemblies());
            Log.Warning("===============热更逻辑加载成功{0}==============", DateTime.Now);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}