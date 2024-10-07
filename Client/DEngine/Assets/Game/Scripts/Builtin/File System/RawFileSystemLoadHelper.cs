using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DEngine.FileSystem;
using UnityEngine;

namespace Game.FileSystem
{
    public class RawFileSystemLoadHelper
    {
        private Dictionary<string, IFileSystem> m_FileSystems = new();
    }
}