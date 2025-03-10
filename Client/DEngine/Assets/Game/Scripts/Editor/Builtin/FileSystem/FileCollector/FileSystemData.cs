﻿using System;
using System.Collections.Generic;
using System.IO;
using DEngine;
using UnityEditor;

namespace Game.Editor.FileSystem
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
        public string OutPutFolderPath = null;

        /// <summary>
        /// 资源对象
        /// </summary>
        public List<string> FileFullPaths = null;

        /// <summary>
        /// 是否使用加密
        /// </summary>
        public bool UseEncryption = false;

        /// <summary>
        /// 是否是有效资源
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(FileSystem) && AssetDatabase.IsValidFolder(OutPutFolderPath);
    }
}