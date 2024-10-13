using System.Collections.Generic;
using System.IO;
using DEngine;
using Game.FileSystem;
using UnityEditor;

namespace Game.Editor.FileSystem
{
    public class FileSystemCollectorTask : IFileSystemTask
    {
        public void Run(FileSystemTaskRunner runner)
        {
            var fileSystemCollector = FileSystemCollector.Instance;
            Dictionary<string, FileSystemDataVersion> fileSystemVersions = new();

            foreach (var fileSystemData in fileSystemCollector.FileSystemDatas)
            {
                if (string.IsNullOrWhiteSpace(fileSystemData.FileSystem))
                {
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(fileSystemData.OutPutFolderPath))
                {
                    continue;
                }

                if (fileSystemData.FileFullPaths is not { Count: > 0 })
                {
                    continue;
                }

                string fullPath = Utility.Path.GetRegularCombinePath(fileSystemData.OutPutFolderPath, fileSystemData.FileSystem + ".bytes");
                string versionPath = Utility.Path.GetRegularCombinePath(fileSystemData.OutPutFolderPath, fileSystemData.FileSystem + "Version.bytes");
                fileSystemVersions[versionPath] = runner.Execute(fullPath, fileSystemData.FileFullPaths);
            }

            foreach (var version in fileSystemVersions)
            {
                File.WriteAllBytes(version.Key, FileSystemDataVersion.ToBson(version.Value));
                File.WriteAllText(version.Key.Replace(".bytes", ".json"), FileSystemDataVersion.ToJson(version.Value));
            }
        }
    }
}