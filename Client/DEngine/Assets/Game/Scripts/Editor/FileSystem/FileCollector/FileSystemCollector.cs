using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [CreateAssetMenu(menuName = "Game/FileSystem Collector", order = 3)]
    public class FileSystemCollector : ScriptableObject
    {
        public int FileSystemHandlerTypeNameIndex;

        public string FileSystemHelperTypeName;

        public List<FileSystemData> FileSystemDatas = new();
    }
}