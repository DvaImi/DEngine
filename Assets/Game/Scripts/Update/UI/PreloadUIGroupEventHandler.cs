using DEngine.Runtime;

namespace Game.Update
{
    public class PreloadUIGroupEventHandler : IPreloadEventHandler
    {
        public void Run()
        {
            DRUIGroup[] uiGroups = GameEntry.DataTable.GetDataTable<DRUIGroup>().GetAllDataRows();

            for (int i = 0; i < uiGroups.Length; i++)
            {
                if (GameEntry.UI.AddUIGroup(uiGroups[i].UIGroupName, uiGroups[i].UIGroupDepth))
                {
                    Log.Info("add ui group [{0}] success", uiGroups[i].UIGroupName);
                }
                else
                {
                    Log.Warning("add ui group [{0}] failure", uiGroups[i].UIGroupName);
                }
            }
        }
    }
}
