using DEngine.Procedure;
using DEngine.Runtime;
using ProcedureOwner = DEngine.Fsm.IFsm<DEngine.Procedure.IProcedureManager>;

namespace Game.Update.Procedure
{
    public class ProcedureProcessingPreload : ProcedureBase
    {
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            ProcessUIGroup();
            ProcessEntityGroup();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedureGame>(procedureOwner);
        }

        private void ProcessUIGroup()
        {
            Log.Info("Process ui group");
            var uiGroups = GameEntry.DataTable.GetDataTable<DRUIGroup>().GetAllDataRows();

            foreach (var group in uiGroups)
            {
                if (GameEntry.UI.AddUIGroup(group.UIGroupName, group.UIGroupDepth))
                {
                    Log.Info("Add ui group [{0}] success", group.UIGroupName);
                }
                else
                {
                    Log.Warning("Add ui group [{0}] failure", group.UIGroupName);
                }
            }
        }

        private void ProcessEntityGroup()
        {
            Log.Info("Process entity group");
        }
    }
}