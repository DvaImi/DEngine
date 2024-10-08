using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine;
using DEngine.FileSystem;
using Game.Editor.ResourceTools;
using Game.FileSystem;
using Game.Update;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public static partial class GameBuildPipeline
    {
        /// <summary>
        /// 导出文件系统
        /// </summary>
        /// <param name="fileSystemHelper"></param>
        /// <param name="fileSystemDataHandlerHelper"></param>
        /// <param name="fileSystemCollector"></param>
        public static void ExportFileSystem(IFileSystemHelper fileSystemHelper, IFileSystemDataHandlerHelper fileSystemDataHandlerHelper, FileSystemCollector fileSystemCollector)
        {
            var fileSystemManager = DEngineEntry.GetModule<IFileSystemManager>();
            fileSystemManager.SetFileSystemHelper(fileSystemHelper);
            Dictionary<string, IFileSystem> fileSystemMap = new();
            Dictionary<string, FileSystemDataVersion> fileSystemVersions = new();
            int totalFiles = fileSystemCollector.FileSystemDatas.Sum(rfd => rfd.AssetPaths.Count);
            int processedFiles = 0;

            try
            {
                foreach (FileSystemData rawFileDefine in fileSystemCollector.FileSystemDatas)
                {
                    if (!fileSystemMap.TryGetValue(rawFileDefine.FileSystem, out var fileSystem))
                    {
                        fileSystem = fileSystemManager.CreateFileSystem(UpdateAssetUtility.GetFileSystemAsset(rawFileDefine.FileSystem), FileSystemAccess.Write, rawFileDefine.AssetPaths.Count, rawFileDefine.AssetPaths.Count);
                        fileSystemMap[rawFileDefine.FileSystem] = fileSystem;
                    }

                    if (!fileSystemVersions.TryGetValue(rawFileDefine.FileSystem, out var fileSystemDataVersion))
                    {
                        fileSystemDataVersion = new FileSystemDataVersion(rawFileDefine.FileSystem, UpdateAssetUtility.GetFileSystemAsset(rawFileDefine.FileSystem));
                        fileSystemVersions[rawFileDefine.FileSystem] = fileSystemDataVersion;
                        fileSystemDataVersion.FileCount = rawFileDefine.AssetPaths.Count;
                    }

                    foreach (string assetPath in rawFileDefine.AssetPaths)
                    {
                        if (AssetDatabase.IsValidFolder(assetPath))
                        {
                            string[] allFiles = Directory.GetFiles(assetPath, "*.*", SearchOption.AllDirectories).Where(file => !file.EndsWith(".meta")).ToArray();
                            foreach (string fullPath in allFiles)
                            {
                                byte[] data = fileSystemDataHandlerHelper.GetBytes(fullPath);
                                fileSystem.WriteFile(UpdateAssetUtility.GetFileSystemAsset(rawFileDefine.FileSystem), data);
                                fileSystemDataVersion.Length += data.LongLength;
                                fileSystemDataVersion.AssetNames.Add(Path.GetFileNameWithoutExtension(fullPath));
                                fileSystemDataVersion.AssetFullNames.Add(Utility.Path.GetRegularPath(fullPath));
                                processedFiles++;
                                float progress = (float)processedFiles / totalFiles;
                                EditorUtility.DisplayProgressBar("Exporting Files", $"Processing {fullPath}", progress);
                            }
                        }
                        else
                        {
                            if (!assetPath.EndsWith(".meta"))
                            {
                                byte[] data = fileSystemDataHandlerHelper.GetBytes(assetPath);
                                fileSystem.WriteFile(UpdateAssetUtility.GetFileSystemAsset(rawFileDefine.FileSystem), data);
                                fileSystemDataVersion.Length += data.LongLength;
                                fileSystemDataVersion.AssetNames.Add(Path.GetFileNameWithoutExtension(assetPath));
                                fileSystemDataVersion.AssetFullNames.Add(Utility.Path.GetRegularPath(assetPath));
                                processedFiles++;
                                float progress = (float)processedFiles / totalFiles;
                                EditorUtility.DisplayProgressBar("Exporting Files", $"Processing {assetPath}", progress);
                            }
                        }
                    }
                }

                GameAssetVersionUitlity.CreateAssetVersion(fileSystemVersions, UpdateAssetUtility.GetFileSystemAsset(nameof(FileSystemDataVersion)));
            }
            finally
            {
                foreach (var fileSystem in fileSystemMap)
                {
                    fileSystemManager.DestroyFileSystem(fileSystem.Value, false);
                }

                EditorUtility.ClearProgressBar();
                DEngineEntry.Shutdown();
                AssetDatabase.Refresh();
                Debug.Log("Export complete.");
            }
        }
    }
}