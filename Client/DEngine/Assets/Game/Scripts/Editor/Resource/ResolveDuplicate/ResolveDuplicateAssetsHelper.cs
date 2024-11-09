using System.Collections.Generic;
using System.Linq;
using DEngine.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public static class ResolveDuplicateAssetsHelper
    {
        private const string SharedAssetBundleName = "SharedAssets";

        public static bool ResolveDuplicateAssets(ResourceEditorController editorController)
        {
            if (editorController.HasResource(SharedAssetBundleName, null))
            {
                foreach (var item in editorController.GetResource(SharedAssetBundleName, null).GetAssets())
                {
                    editorController.UnassignAsset(item.Guid);
                }

                editorController.Save();
            }

            var duplicateAssetNames = FindDuplicateAssetNames();
            return duplicateAssetNames is not { Count: > 0 } || ResolveDuplicateAssets(editorController, duplicateAssetNames);
        }

        private static bool ResolveDuplicateAssets(ResourceEditorController editorController, List<string> duplicateAssetNames)
        {
            if (!editorController.HasResource(SharedAssetBundleName, null))
            {
                bool addSuccess = editorController.AddResource(SharedAssetBundleName, null, null, LoadType.LoadFromMemoryAndQuickDecrypt, false);

                if (!addSuccess)
                {
                    Debug.LogWarningFormat("ResourceEditor Add Resource:{0} Failed!", SharedAssetBundleName);
                    return false;
                }
            }

            var sharedRes = editorController.GetResource(SharedAssetBundleName, null);
            List<string> sharedResFiles = new List<string>();
            foreach (var item in sharedRes.GetAssets())
            {
                sharedResFiles.Add(item.Name);
            }

            foreach (var assetName in duplicateAssetNames)
            {
                Debug.Log($"冗余资源:{assetName}");
                if (sharedResFiles.Contains(assetName))
                {
                    continue;
                }

                if (!editorController.AssignAsset(AssetDatabase.AssetPathToGUID(assetName), SharedAssetBundleName, null))
                {
                    Debug.LogWarning($"添加资源:{assetName}到{SharedAssetBundleName}失败!");
                }
            }

            Debug.Log($"-------------处理冗余资源结束------------");
            var sharedAseets = sharedRes.GetAssets();
            for (int i = sharedAseets.Length - 1; i >= 0; i--)
            {
                var asset = sharedAseets[i];
                if (!duplicateAssetNames.Contains(asset.Name))
                {
                    if (!editorController.UnassignAsset(asset.Guid))
                    {
                        Debug.LogWarning($"移除{SharedAssetBundleName}中的资源:{asset.Name}失败!");
                    }
                }
            }

            return true;
        }

        private static List<string> FindDuplicateAssetNames()
        {
            ResourceAnalyzerController resAnalyzer = new ResourceAnalyzerController();
            if (resAnalyzer.Prepare())
            {
                resAnalyzer.Analyze();
                List<string> duplicateAssets = new List<string>();
                var scatteredAssetNames = resAnalyzer.GetScatteredAssetNames();
                foreach (var scatteredAsset in scatteredAssetNames)
                {
                    var hostAssets = resAnalyzer.GetHostAssets(scatteredAsset);
                    if (hostAssets == null || hostAssets.Length < 1) continue;
                    var defaultHostAsset = hostAssets.FirstOrDefault(res => res.Resource.FullName != SharedAssetBundleName);
                    if (defaultHostAsset != null)
                    {
                        var hostResourceName = defaultHostAsset.Resource.FullName;
                        if (hostAssets.Where(hostAsset => hostAsset.Resource.FullName != SharedAssetBundleName).Any(hostAsset => hostResourceName != hostAsset.Resource.Name))
                        {
                            duplicateAssets.Add(scatteredAsset);
                        }
                    }
                }

                return duplicateAssets;
            }

            return null;
        }
    }
}