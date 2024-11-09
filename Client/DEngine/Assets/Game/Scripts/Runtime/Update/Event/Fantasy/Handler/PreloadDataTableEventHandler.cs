using Fantasy.Async;
using Fantasy.Event;
using Game.Update.DataTable;
using GameEntry = Game.GameEntry;

public class PreloadDataTableEventHandler : AsyncEventSystem<PreloadEventType>
{
    protected override async FTask Handler(PreloadEventType self)
    {
        await GameEntry.DataTable.LoadAllDataTableAsync();
    }
}