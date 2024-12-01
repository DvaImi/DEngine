using DEngine.Fsm;
using DEngine.Procedure;
using DEngine.Resource;
using DEngine.Runtime;
using Game.Debugger;

namespace Game
{
    public class ProcedureLaunch : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.BuiltinData.InitLanguageSettings();
            BuiltinDataComponent.InitCurrentVariant();
            BuiltinDataComponent.InitSoundSettings();
            BuiltinDataComponent.InitExtensionEventHandle();

            GameEntry.Debugger.RegisterDebuggerWindow("Other/Language", new ChangeLanguageDebuggerWindow());
            GameEntry.Debugger.RegisterDebuggerWindow("Other/CommonLine", new CommonLineDebuggerWindow());
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

#if UNITY_EDITOR
            if (GameEntry.Base.EditorResourceMode)
            {
                // 编辑器模式
                Log.Info("Editor resource mode detected.");
                ChangeState<ProcedureLoadHotUpdateEntry>(procedureOwner);
                return;
            }
#endif
            if (GameEntry.Resource.ResourceMode == ResourceMode.Package)
            {
                // 单机模式
                Log.Info("Package resource mode detected.");
                ChangeState<ProcedureInitResources>(procedureOwner);
            }
            else
            {
                // 可更新模式
                Log.Info("Updatable resource mode detected.");
                Log.Info(GameEntry.BuiltinData.ForceCheckVersion ? "Current updatable resource mode enforces version check." : "Current updatable resource mode does not enforce version check.");
                ChangeState<ProcedureCheckVersion>(procedureOwner);
            }
        }
    }
}