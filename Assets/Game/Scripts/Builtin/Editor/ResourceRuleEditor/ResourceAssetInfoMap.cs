// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-24 20:36:31
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.IO;
using GameFramework;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using UnityGameFramework.Runtime;

namespace Game.Editor
{
    public static class ResourceAssetInfoMap
    {
        public static void ConvertMap()
        {
            ResourceCollection resourceCollection = new ResourceCollection();

            if (resourceCollection.Load())
            {
                var assets = resourceCollection.GetAssets();

                if (assets == null)
                {
                    return;
                }

                AssetInfoMap assetInfo = new()
                {
                    AssetPathMap = new Dictionary<string, HashSet<string>>(),
                    AssetSuffixMap = new Dictionary<string, HashSet<string>>()
                };
                for (int i = 0; i < assets.Length; i++)
                {
                    string key = Path.GetFileName(Path.GetDirectoryName(assets[i].Resource.Name));
                    if (assetInfo.AssetPathMap.ContainsKey(key))
                    {
                       
                        assetInfo.AssetPathMap[key].Add(Utility.Path.GetRegularPath(Path.GetDirectoryName(assets[i].Name)));
                    }
                    else
                    {
                        assetInfo.AssetPathMap.Add(key, new HashSet<string>() { Utility.Path.GetRegularPath(Path.GetDirectoryName(assets[i].Name)) });
                    }

                    if (assetInfo.AssetSuffixMap.ContainsKey(key))
                    {
                         assetInfo.AssetSuffixMap[key].Add(Path.GetExtension(assets[i].Name));
                    }
                    else
                    {
                        assetInfo.AssetSuffixMap.Add(key, new HashSet<string>() { Path.GetExtension(assets[i].Name) });
                    }
                }

                string assetInfoJson = JsonConvert.SerializeObject(assetInfo);
                IOUtility.SaveFileSafe("Assets/GameMain/AssetBundle/Configs/Runtime/AssetInfo.txt", assetInfoJson);
                AssetDatabase.Refresh();
                Debug.Log("ConvertAssetMap success " + assetInfoJson);
            }
        }
    }
}
