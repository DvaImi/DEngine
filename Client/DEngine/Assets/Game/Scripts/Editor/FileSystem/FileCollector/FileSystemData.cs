using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Game.Editor.ResourceTools
{
    [Serializable]
    public class FileSystemData
    {
        /// <summary>
        /// 资源文件系统
        /// </summary>
        public string FileSystem = null;

        /// <summary>
        /// 资源对象
        /// </summary>
        public List<Object> AssetObjects = null;
    }
}