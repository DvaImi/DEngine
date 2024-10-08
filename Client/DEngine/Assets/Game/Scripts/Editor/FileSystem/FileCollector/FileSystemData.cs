using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
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
        /// 导出路径
        /// </summary>
        public string OutPutPath = null;

        /// <summary>
        /// 资源对象
        /// </summary>
        public List<string> AssetPaths = null;
    }
}