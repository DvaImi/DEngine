using DEngine.Runtime;
using Fantasy.Async;
using Fantasy.Event;
using Game.Update;
using GameEntry = Game.GameEntry;

public class ProcessingUIGroupEventHandler : AsyncEventSystem<ProcessingPreloadEventType>
{
    protected override async FTask Handler(ProcessingPreloadEventType self)
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
        await FTask.CompletedTask;
    }
}