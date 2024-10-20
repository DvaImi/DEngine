using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Resource
{
    public static class GameAssetVersionLoader
    {
        /// <summary>
        /// 加载资源列表
        /// </summary>
        /// <param name="assetVersionName"></param>
        /// <returns></returns>
        public static async UniTask<string[]> LoadAsync(string assetVersionName)
        {
            TextAsset textAsset = await GameEntry.Resource.LoadAssetAsync<TextAsset>(assetVersionName);
            if (!textAsset)
            {
                return null;
            }

            await using Stream stream = new MemoryStream(textAsset.bytes);
            using var binaryReader = new BinaryReader(stream, Encoding.UTF8);
            var length = binaryReader.ReadInt32();
            var result = new string[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = binaryReader.ReadString();
            }

            return result;
        }
    }
}