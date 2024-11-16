using System.Collections.Generic;
using System.Linq;
using DEngine.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public class DefaultResolveDuplicateAssetsHelper : IResolveDuplicateAssetsHelper
    {
        private const string SharedAssetGroupName = "SharedAssets";

        public List<string> GetDuplicateAssetNames()
        {
            ResourceAnalyzerController resAnalyzer = new ResourceAnalyzerController();
            if (resAnalyzer.Prepare())
            {
                resAnalyzer.Analyze();
                List<string> duplicateAssets     = new List<string>();
                var          scatteredAssetNames = resAnalyzer.GetScatteredAssetNames();
                foreach (var scatteredAsset in scatteredAssetNames)
                {
                    var hostAssets = resAnalyzer.GetHostAssets(scatteredAsset);
                    if (hostAssets == null || hostAssets.Length < 1)
                    {
                        continue;
                    }

                    duplicateAssets.Add(scatteredAsset);
                }

                return duplicateAssets;
            }

            return null;
        }

        public bool ResolveDuplicateAssets(List<string> duplicateAssetNames)
        {
            var packageCollector = ResourcePackagesCollector.GetResourceGroupsCollector();
            var sharedAssetGroup = packageCollector.Groups.FirstOrDefault(o => o.GroupName == SharedAssetGroupName);
            if (sharedAssetGroup == null)
            {
                sharedAssetGroup = new ResourceGroupCollector
                {
                    GroupName   = SharedAssetGroupName,
                    Description = "自动处理的冗余资源"
                };
                packageCollector.Groups.Add(sharedAssetGroup);
            }

            foreach (var assetName in duplicateAssetNames)
            {
                string duplicateAssetGuid = AssetDatabase.AssetPathToGUID(assetName);
                if (string.IsNullOrEmpty(duplicateAssetGuid))
                {
                    Debug.LogWarning($"无效的资源路径: {assetName}");
                    continue;
                }

                if (sharedAssetGroup.AssetCollectors.Any(o => duplicateAssetGuid == AssetDatabase.AssetPathToGUID(o.AssetPath)))
                {
                    Debug.Log($"资源已存在于共享组中: {assetName}");
                    continue;
                }

                // 移除其他组中重复的资源
                RemoveDuplicateFromGroups(packageCollector, duplicateAssetGuid);

                // 添加到共享资源组
                sharedAssetGroup.AssetCollectors.Add(new ResourceCollector
                {
                    Name       = "SharedAssets",
                    FileSystem = "SharedAssets",
                    Groups     = SharedAssetGroupName,
                    Asset      = AssetDatabase.LoadAssetAtPath<Object>(assetName)
                });
                Debug.Log($"冗余资源已移动到共享组: {assetName}");
            }

            ResourcePackagesCollector.Save();
            Debug.Log("-------------处理冗余资源结束------------");
            return true;
        }

        private static void RemoveDuplicateFromGroups(ResourceGroupsCollector packageCollector, string duplicateAssetGuid)
        {
            foreach (var resourceGroup in packageCollector.Groups)
            {
                resourceGroup.AssetCollectors.RemoveAll((o) => Predicate(o, duplicateAssetGuid));
            }
        }

        private static bool Predicate(ResourceCollector collector, string duplicateAssetGuid)
        {
            var assetGuid = AssetDatabase.AssetPathToGUID(collector.AssetPath);
            return !string.IsNullOrEmpty(assetGuid) && assetGuid == duplicateAssetGuid;
        }
    }
}