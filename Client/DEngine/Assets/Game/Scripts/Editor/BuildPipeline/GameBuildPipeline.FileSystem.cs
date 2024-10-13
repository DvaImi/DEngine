using System.Linq;
using Game.Editor.FileSystem;
using UnityEditor;

namespace Game.Editor.BuildPipeline
{
    /// <summary>
    /// 文件系统资源生成器。
    /// </summary>
    public static partial class GameBuildPipeline
    {
        public static void ProcessFileSystem()
        {
            var fileSystemList = FileSystemCollector.Instance.FileSystemDatas;
            foreach (var fileSystem in fileSystemList.Where(fileSystem => fileSystem.IsValid))
            {
                GameUtility.IO.Delete(fileSystem.OutPutFolderPath);
            }

            AssetDatabase.Refresh();
            new FileSystemTaskRunner().Run();
        }
    }
}