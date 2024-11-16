using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    internal static class ResourceCollectorEditorUtility
    {
        /// <summary>
        /// 资源规则
        /// </summary>
        private static ResourceCollection s_ResourceCollection;

        /// <summary>
        /// 排除的类型
        /// </summary>
        private static string[] s_SourceAssetExceptTypeFilterGuidArray;

        /// <summary>
        /// 排除的标签
        /// </summary>
        private static string[] s_SourceAssetExceptLabelFilterGuidArray;

        /// <summary>
        /// 缓存收集规则类型
        /// </summary>
        private static readonly Dictionary<string, Type> CacheFilterRuleTypes = new();

        /// <summary>
        /// 缓存收集规则实例
        /// </summary>
        private static readonly Dictionary<string, IFilterRule> CacheFilterRuleInstance = new();

        /// <summary>
        /// 资源收集规则名称
        /// </summary>
        public static string[] FilterRules { get; }

        /// <summary>
        /// 资源规则
        /// </summary>
        private static ResourceCollection Collection
        {
            get
            {
                if (s_ResourceCollection == null)
                {
                    s_ResourceCollection = new ResourceCollection();
                    if (!s_ResourceCollection.Load())
                    {
                        s_ResourceCollection.Save();
                    }
                }

                return s_ResourceCollection;
            }
        }

        private static readonly IResolveDuplicateAssetsHelper ResolveDuplicateAssetsHelper;

        static ResourceCollectorEditorUtility()
        {
            CacheFilterRuleTypes.Clear();
            CacheFilterRuleInstance.Clear();
            List<Type> types = EditorTools.GetAssignableTypes(typeof(IFilterRule));
            FilterRules = new string[types.Count];
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (!CacheFilterRuleTypes.TryAdd(type.Name, type))
                {
                    continue;
                }

                FilterRules[i] = type.Name;
            }


            var resolveDuplicateAssetsHelperType = Utility.Assembly.GetType(EditorPrefs.GetString("Resolve DuplicateAssets Helper Type Name"));
            if (resolveDuplicateAssetsHelperType == null)
            {
                Debug.LogError("Failed to get ResolveDuplicateAssetsHelper Type.");
                return;
            }

            ResolveDuplicateAssetsHelper = (IResolveDuplicateAssetsHelper)Activator.CreateInstance(resolveDuplicateAssetsHelperType);
            if (ResolveDuplicateAssetsHelper == null)
            {
                Debug.LogWarning("Failed to create ResolveDuplicateAssetsHelper instance.");
            }
        }

        /// <summary>
        /// 更新资源收集器
        /// </summary>
        /// <param name="collectorData"></param>
        public static void RefreshResourceCollection(ResourceGroupsCollector collectorData = null)
        {
            ResourceEditorController resourceEditorController = new();

            if (resourceEditorController.Load())
            {
                collectorData ??= ResourcePackagesCollector.GetResourceGroupsCollector(DEngineSetting.Instance.AssetBundleCollectorIndex);
                Debug.Log($"Export {collectorData.PackageName} ...");

                s_SourceAssetExceptTypeFilterGuidArray  = AssetDatabase.FindAssets(resourceEditorController.SourceAssetExceptTypeFilter);
                s_SourceAssetExceptLabelFilterGuidArray = AssetDatabase.FindAssets(resourceEditorController.SourceAssetExceptLabelFilter);

                Collection.Clear();
                AnalysisResourceFilters(collectorData);
                int unknownAssetCount   = resourceEditorController.RemoveUnknownAssets();
                int unusedResourceCount = resourceEditorController.RemoveUnusedResources();
                Debug.Log(Utility.Text.Format("Clean complete, {0} unknown assets and {1} unused resources has been removed.", unknownAssetCount, unusedResourceCount));


                resourceEditorController.Save();
                Debug.Log(Collection.Save() ? "Refresh ResourceCollection.xml success" : "Refresh ResourceCollection.xml failure");
                AssetDatabase.Refresh();
                return;
            }

            if (!resourceEditorController.Save())
            {
            }
        }

        /// <summary>
        /// 获取指定规则的实例
        /// </summary>
        /// <param name="ruleName">规则名称</param>
        /// <returns>过滤规则实例</returns>
        private static IFilterRule GetFilterRuleInstance(string ruleName)
        {
            if (CacheFilterRuleInstance.TryGetValue(ruleName, out IFilterRule instance))
            {
                return instance;
            }

            if (CacheFilterRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IFilterRule)Activator.CreateInstance(type);
                CacheFilterRuleInstance.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IFilterRule)}类型无效:{ruleName}");
        }

        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns>资源数组</returns>
        private static Resource[] GetResources()
        {
            return Collection.GetResources();
        }

        /// <summary>
        /// 检查是否存在指定的资源
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <param name="variant">资源变体</param>
        /// <returns>是否存在</returns>
        private static bool HasResource(string name, string variant)
        {
            return Collection.HasResource(name, variant);
        }

        /// <summary>
        /// 添加新资源
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <param name="variant">资源变体</param>
        /// <param name="fileSystem">文件系统名称</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="packed">是否打包</param>
        /// <param name="resourceGroups">资源组数组</param>
        /// <returns>添加是否成功</returns>
        private static bool AddResource(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            return Collection.AddResource(name, variant, fileSystem, loadType, packed, resourceGroups);
        }

        /// <summary>
        /// 重命名资源
        /// </summary>
        /// <param name="oldName">旧资源名称</param>
        /// <param name="oldVariant">旧资源变体</param>
        /// <param name="newName">新资源名称</param>
        /// <param name="newVariant">新资源变体</param>
        /// <returns>重命名是否成功</returns>
        private static bool RenameResource(string oldName, string oldVariant, string newName, string newVariant)
        {
            return Collection.RenameResource(oldName, oldVariant, newName, newVariant);
        }

        /// <summary>
        /// 分配资源给指定的Asset
        /// </summary>
        /// <param name="assetGuid">Asset的GUID</param>
        /// <param name="resourceName">资源名称</param>
        /// <param name="resourceVariant">资源变体</param>
        /// <returns>分配是否成功</returns>
        private static bool AssignAsset(string assetGuid, string resourceName, string resourceVariant)
        {
            return Collection.AssignAsset(assetGuid, resourceName, resourceVariant);
        }

        /// <summary>
        /// 分析资源过滤器并应用规则
        /// </summary>
        /// <param name="collectorData">资源组收集器</param>
        private static void AnalysisResourceFilters(ResourceGroupsCollector collectorData)
        {
            var duplicateAssetNames = ResolveDuplicateAssetsHelper?.GetDuplicateAssetNames();
            if (duplicateAssetNames is { Count: > 0 } && ResolveDuplicateAssetsHelper.ResolveDuplicateAssets(duplicateAssetNames))
            {
                Debug.Log("resolve duplicate assets successfully.");
            }

            ResourceEditorController resourceEditorController = new();

            string sourceAssetRootPath = "Assets";
            if (resourceEditorController.Load())
            {
                sourceAssetRootPath = resourceEditorController.SourceAssetRootPath;
            }

            foreach (var resourceGroup in collectorData.Groups)
            {
                if (resourceGroup.EnableGroup)
                {
                    foreach (var resourceCollector in resourceGroup.AssetCollectors)
                    {
                        if (!resourceCollector.Enable)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(resourceCollector.AssetPath))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(resourceCollector.Variant))
                        {
                            resourceCollector.Variant = null;
                        }

                        string resourceName = string.IsNullOrEmpty(resourceCollector.Name) ? resourceCollector.AssetPath[(sourceAssetRootPath.Length + 1)..] : resourceCollector.Name;

                        if (AssetDatabase.IsValidFolder(resourceCollector.AssetPath))
                        {
                            FileInfo[] assetFiles = new DirectoryInfo(resourceCollector.AssetPath).GetFiles("*.*", SearchOption.AllDirectories);
                            if (assetFiles.Length > 0)
                            {
                                ProcessFolderResource(resourceCollector, resourceName, GetFilterRuleInstance(resourceCollector.FilterRule), assetFiles);
                            }
                        }
                        else if (File.Exists(resourceCollector.AssetPath))
                        {
                            ProcessFileFilter(resourceCollector, resourceName, GetFilterRuleInstance(resourceCollector.FilterRule));
                        }
                        else
                        {
                            // 路径无效
                            Debug.LogWarningFormat("assetPath {0} is invalid.", resourceCollector.AssetPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  处理文件夹资源
        /// </summary>
        /// <param name="resourceCollector"></param>
        /// <param name="resourceName"></param>
        /// <param name="filterRule"></param>
        /// <param name="assetFiles"></param>
        private static void ProcessFolderResource(ResourceCollector resourceCollector, string resourceName, IFilterRule filterRule, FileInfo[] assetFiles)
        {
            TryAddResource(resourceCollector, resourceName);
            foreach (var file in assetFiles)
            {
                if (filterRule.IsCollectAsset(file.FullName))
                {
                    string assetName = Path.Combine("Assets", file.FullName[(Application.dataPath.Length + 1)..]);
                    string assetGuid = AssetDatabase.AssetPathToGUID(assetName);
                    if (!s_SourceAssetExceptTypeFilterGuidArray.Contains(assetGuid) && !s_SourceAssetExceptLabelFilterGuidArray.Contains(assetGuid))
                    {
                        if (!AssignAsset(assetGuid, resourceName, resourceCollector.Variant))
                        {
                            Debug.LogWarningFormat("Assign asset '{0}' to resource '{1}' failure.", resourceCollector.Name, resourceCollector.AssetPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理文件
        /// </summary>
        /// <param name="assetCollector">资源收集器</param>
        /// <param name="resourceName">资源名称</param>
        /// <param name="filterRule">过滤规则</param>
        private static void ProcessFileFilter(ResourceCollector assetCollector, string resourceName, IFilterRule filterRule)
        {
            TryAddResource(assetCollector, resourceName);

            if (filterRule.IsCollectAsset(assetCollector.AssetPath))
            {
                string assetGuid = AssetDatabase.AssetPathToGUID(assetCollector.AssetPath);
                if (!s_SourceAssetExceptTypeFilterGuidArray.Contains(assetGuid) && !s_SourceAssetExceptLabelFilterGuidArray.Contains(assetGuid))
                {
                    if (!AssignAsset(assetGuid, resourceName, assetCollector.Variant))
                    {
                        Debug.LogWarningFormat("Assign asset '{0}' to resource '{1}' failure.", assetCollector.Name, assetCollector.AssetPath);
                    }
                }
            }
        }

        private static void TryAddResource(ResourceCollector assetCollector, string resourceName)
        {
            foreach (var oldResource in GetResources())
            {
                if (oldResource.Name == resourceName && string.IsNullOrEmpty(oldResource.Variant))
                {
                    if (!RenameResource(oldResource.Name, oldResource.Variant, resourceName, assetCollector.Variant))
                    {
                        Debug.LogWarningFormat("Rename resource '{0}' to '{1}' failure.", oldResource.Name, resourceName);
                    }

                    break;
                }
            }

            if (!HasResource(resourceName, null))
            {
                if (string.IsNullOrEmpty(assetCollector.FileSystem))
                {
                    assetCollector.FileSystem = null;
                }

                if (!AddResource(resourceName, assetCollector.Variant, assetCollector.FileSystem, assetCollector.LoadType, assetCollector.Packed, assetCollector.Groups.Split('|')))
                {
                    Debug.LogWarningFormat("Add resource '{0}' failure.", assetCollector.AssetPath);
                }
            }
        }
    }
}