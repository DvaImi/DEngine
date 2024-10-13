using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public FileSystemDataVersion Execute(string fileSystemFullPath, IList<string> sourceFileFullPaths)
        {
            int processedFiles = 0;
            int totalFiles = sourceFileFullPaths.Count;
            var fileSystem = GetFileSystem(fileSystemFullPath, totalFiles, totalFiles);
            var fileSystemDataVersion = new FileSystemDataVersion
            {
                FileSystem = fileSystem.FullPath
            };


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
                fileSystem.WriteFile(fileName, data);
                processedFiles++;
                var progress = (float)processedFiles / totalFiles;
                EditorUtility.DisplayProgressBar("Build Files", $"Processing {fullPath}", progress);
            }

            foreach (var fileInfo in fileSystem.GetAllFileInfos())
            {
                fileSystemDataVersion.FileInfos[fileInfo.Name] = new FileInfo(fileInfo.Offset, fileInfo.Length);
            }

            return fileSystemDataVersion;
        }

        public void Run()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
            var taskTypes = allTypes.Where(type => typeof(IFileSystemTask).IsAssignableFrom(type) && !type.IsAbstract);
            var buildFileSystemTasks = taskTypes.Select(taskType => (IFileSystemTask)Activator.CreateInstance(taskType)).Where(instance => instance != null).ToList();

            try
            {
                foreach (var task in buildFileSystemTasks)
                {
                    task.Run(this);
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