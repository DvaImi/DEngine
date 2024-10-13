using System.IO;
using DEngine.Runtime;
using Game.FileSystem;
using Luban;

namespace Game.Update.DataTable
{
    public sealed class LubanDataProvider : ILubanDataProvider
    {
        public int Priority => 1;

        public Tables Tables { get; }

        private readonly FileSystemDataVersion m_FileSystemDataVersion;
        private readonly string m_FileSystemFullPath;


        public LubanDataProvider()
        {
            Tables = new Tables(OnLoadByteBuf);
            if (GameEntry.Base.EditorResourceMode)
            {
                return;
            }

            m_FileSystemFullPath = UpdateAssetUtility.GetConfigAsset("cfg");
            m_FileSystemDataVersion = FileSystemDataVersion.Deserialize(GameEntry.Resource.LoadBinaryFromFileSystem(UpdateAssetUtility.GetConfigAsset("cfgVersion")));
        }

        #region Lazy Load

        private ByteBuf OnLoadByteBuf(string tableName)
        {
            if (GameEntry.Base.EditorResourceMode)
            {
                return new ByteBuf(File.ReadAllBytes($"Assets/../../../Share/Luban/Client/output/Bin/{tableName}.bytes"));
            }

            if (m_FileSystemDataVersion.FileInfos.TryGetValue(tableName, out var info))
            {
                var bytes = GameEntry.Resource.LoadBinarySegmentFromFileSystem(m_FileSystemFullPath, (int)info.Offset, info.Length);
                return new ByteBuf(bytes);
            }

            Log.Error($"FileSystem does not have this '{tableName}' dataTable");
            return new ByteBuf();
        }

        #endregion

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Shutdown()
        {
        }
    }
}