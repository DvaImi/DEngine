using System;
using DEngine.Procedure;
using DEngine.Runtime;
using Game.Debugger;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update.Procedure
{
    public class ProcedureLanuchUpdate : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Entry.Initialize();
            Log.Warning("===============热更逻辑加载成功{0}==============", DateTime.Now);
            GameEntry.Debugger.RegisterDebuggerWindow("Profiler/Network", new NetworkDebuggerWindow());
            GameEntry.Debugger.RegisterDebuggerWindow("Other/Language", new ChangeLanguageDebuggerWindow());
            GameEntry.Debugger.RegisterDebuggerWindow("Other/CommonLine", new CommonLineDebuggerWindow());
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedurePreload>(procedureOwner);
        }
    }
}