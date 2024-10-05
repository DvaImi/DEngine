using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public class DefaultFilterRule
    {
        /// <summary>
        /// 忽略的文件类型
        /// </summary>
        private static readonly HashSet<string> IgnoreFileExtensions = new() { ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc", ".hlsl" };

        /// <summary>
        /// 查询是否为忽略文件
        /// </summary>
        public static bool IsIgnoreFile(string fileExtension)
        {
            return IgnoreFileExtensions.Contains(fileExtension);
        }
    }

    public class CollectAll : IFilterRule
    {
        public bool IsCollectAsset(string assetPath)
        {
            return !DefaultFilterRule.IsIgnoreFile(Path.GetExtension(assetPath));
        }
    }

    public class CollectScene : IFilterRule
    {
        public bool IsCollectAsset(string assetPath)
        {
            return Path.GetExtension(assetPath) == ".unity";
        }
    }

    public class CollectPrefab : IFilterRule
    {
        public bool IsCollectAsset(string assetPath)
        {
            return Path.GetExtension(assetPath) == ".prefab";
        }
    }

    public class CollectSprite : IFilterRule
    {
        public bool IsCollectAsset(string assetPath)
        {
            var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (mainAssetType == typeof(Texture2D))
            {
                TextureImporter texImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                return texImporter != null && texImporter.textureType == TextureImporterType.Sprite;
            }
            else
            {
                return false;
            }
        }
    }
}