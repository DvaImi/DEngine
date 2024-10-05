using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Archive
{
    public class SqliteArchiveHelper : IArchiveHelper
    {
        public bool Query(string fileUri)
        {
            throw new System.NotImplementedException();
        }

        public bool Match(string userIdentifier)
        {
            throw new System.NotImplementedException();
        }

        public UniTask SaveAsync(string fileUri, byte[] bytes)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<byte[]> LoadAsync(string fileUri)
        {
            throw new System.NotImplementedException();
        }
    }
}