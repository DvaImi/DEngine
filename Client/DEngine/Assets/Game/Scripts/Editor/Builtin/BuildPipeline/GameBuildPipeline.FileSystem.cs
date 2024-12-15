using System.IO;
using System.Linq;
using DEngine;
using Game.Editor.FileSystem;
using Game.FileSystem;
using UnityEditor;
using UnityEngine;

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
            new FileSystemTaskRunner().ExecuteAll();
        }

        public static void ProcessFileSystem(FileSystemData fileSystemData)
        {
            if (!fileSystemData.IsValid)
            {
                Debug.Log("fileSystem is invalid.");
                return;
            }

            if (fileSystemData.FileFullPaths is not { Count: > 0 })
            {
                return;
            }

            string fullPath = Utility.Path.GetRegularCombinePath(fileSystemData.OutPutFolderPath, fileSystemData.FileSystem + ".bytes");
            string versionPath = Utility.Path.GetRegularCombinePath(fileSystemData.OutPutFolderPath, fileSystemData.FileSystem + "Version.bytes");
            try
            {
                var version = new FileSystemTaskRunner().Execute(fullPath, fileSystemData.FileFullPaths, fileSystemData.UseEncryption);
                File.WriteAllBytes(versionPath, FileSystemDataVersion.ToBson(version));
                File.WriteAllText(versionPath.Replace(".bytes", ".json"), FileSystemDataVersion.ToJson(version));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                DEngineEntry.Shutdown();
                AssetDatabase.Refresh();
                Debug.Log("Export complete.");
            }
        }
    }
}