using DEngine.Runtime;
using Fantasy.Async;
using Fantasy.Event;
using Game.Update;
using GameEntry = Game.GameEntry;

public class ProcessingEntityGroupEventHandler : AsyncEventSystem<ProcessingPreloadEventType>
{
    protected override async FTask Handler(ProcessingPreloadEventType self)
    {
        Log.Info("Process entity group");
        var groups = GameEntry.DataTable.GetDataTable<DREntityGroup>().GetAllDataRows();

        foreach (var group in groups)
        {
            if (GameEntry.Entity.AddEntityGroup(group.Name, group.InstanceAutoReleaseInterval, group.InstanceCapacity, group.InstanceExpireTime, group.InstancePriority))
            {
                Log.Info("Add entity group [{0}] success", group.Name);
            }
            else
            {
                Log.Warning("Add entity group [{0}] failure", group.Name);
            }
        }

        await FTask.CompletedTask;
    }
}