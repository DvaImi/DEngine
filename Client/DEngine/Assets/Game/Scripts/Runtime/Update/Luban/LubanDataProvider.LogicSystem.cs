using Fantasy.Entitas.Interface;
using Game.FileSystem;

namespace Game.Update.DataTable
{
    public partial class LubanDataProvider
    {
        private class LubanDataProviderAwakeSystem : AwakeSystem<LubanDataProvider>
        {
            protected override void Awake(LubanDataProvider self)
            {
                self.Tables = new Tables(self.OnLoadByteBuf);
                if (GameEntry.Base.EditorResourceMode)
                {
                    return;
                }

                self.m_FileSystemFullPath = UpdateAssetUtility.GetConfigAsset("cfg");
                self.m_FileSystemDataVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(UpdateAssetUtility.GetConfigAsset("cfgVersion")));
            }
        }
    }
}