using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.FileSystem
{
    [GameFilePath("Assets/Game/Configuration/FileSystemCollector.asset"), CreateAssetMenu(menuName = "Game/FileSystem Collector", order = 3)]
    public class FileSystemCollector : ScriptableSingleton<FileSystemCollector>
    {
        public List<FileSystemData> FileSystemDatas = new();

        public FileSystemData Get(string fileSystemName)
        {
            return FileSystemDatas.Find(o => o.FileSystem == fileSystemName);
        }
    }
}