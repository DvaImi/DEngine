using System.Collections.Generic;
using System.IO;
using DEngine;
using DEngine.FileSystem;
using Game.FileSystem;
using UnityEditor;
using UnityEngine;
using FileInfo = Game.FileSystem.FileInfo;

namespace Game.Editor.FileSystem
{
    public class FileSystemTaskRunner
    {
        private readonly IFileSystemManager m_FileSystemManager;

        public FileSystemTaskRunner()
        {
            m_FileSystemManager = DEngineEntry.GetModule<IFileSystemManager>();
            m_FileSystemManager.SetFileSystemHelper(new FileSystemHelper());
        }

        private IFileSystem GetFileSystem(string fullPath, int maxFileCount, int maxBlockCount)
        {
            return m_FileSystemManager.HasFileSystem(fullPath) ? m_FileSystemManager.GetFileSystem(fullPath) : m_FileSystemManager.CreateFileSystem(fullPath, FileSystemAccess.Write, maxFileCount, maxBlockCount);
        }

        public FileSystemDataVersion Execute(string fileSystemFullPath, IList<string> sourceFileFullPaths, bool useEncryption)
        {
            int processedFiles = 0;
            int totalFiles = sourceFileFullPaths.Count;
            var fileSystem = GetFileSystem(fileSystemFullPath, totalFiles, totalFiles);
            var fileSystemDataVersion = new FileSystemDataVersion
            {
                FileSystem = fileSystem.FullPath
            };

            Dictionary<string, int> hashCodeMap = new Dictionary<string, int>();
            foreach (var fullPath in sourceFileFullPaths)
            {
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"SourceFile '{fullPath}' is invalid");
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(fullPath);

                if (fileSystemDataVersion.FileInfos.ContainsKey(fileName))
                {
                    Debug.LogWarning($"There is already a file with the same name '{fileName}'");
                    continue;
                }

                var data = File.ReadAllBytes(fullPath);
                int hashCode = 0;
                if (useEncryption)
                {
                    hashCode = Utility.Verifier.GetCrc32(data);
                    byte[] hashBytes = Utility.Converter.GetBytes(hashCode);
                    data = Utility.Encryption.GetXorBytes(data, hashBytes);
                }

                fileSystem.WriteFile(fileName, data);
                hashCodeMap[fileName] = hashCode;
                processedFiles++;
                var progress = (float)processedFiles / totalFiles;
                EditorUtility.DisplayProgressBar("Build Files", $"Processing {fullPath}", progress);
            }

            foreach (var fileInfo in fileSystem.GetAllFileInfos())
            {
                fileSystemDataVersion.FileInfos[fileInfo.Name] = new FileInfo(fileInfo.Offset, fileInfo.Length, hashCodeMap[fileInfo.Name]);
            }

            return fileSystemDataVersion;
        }

        public void ExecuteAll()
        {
            try
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
                    fileSystemVersions[versionPath] = Execute(fullPath, fileSystemData.FileFullPaths, fileSystemData.UseEncryption);
                }

                foreach (var version in fileSystemVersions)
                {
                    File.WriteAllBytes(version.Key, FileSystemDataVersion.ToBson(version.Value));
                    File.WriteAllText(version.Key.Replace(".bytes", ".json"), FileSystemDataVersion.ToJson(version.Value));
                }
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