using System.IO;
using DEngine.Runtime;
using Game.FileSystem;
using Luban;

namespace Game.Update.DataTable
{
    public partial class LubanDataProvider : Fantasy.Entitas.Entity
    {
        public int Priority => 1;

        public Tables Tables { get; private set; }

        private FileSystemDataVersion m_FileSystemDataVersion;
        private string m_FileSystemFullPath;
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
    }
}