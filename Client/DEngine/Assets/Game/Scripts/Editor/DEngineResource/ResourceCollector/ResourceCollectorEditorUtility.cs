using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static ResourceCollection m_ResourceCollection;
        /// <summary>
        /// 排除的类型
        /// </summary>
        private static string[] m_SourceAssetExceptTypeFilterGUIDArray;
        /// <summary>
        /// 排除的标签
        /// </summary>
        private static string[] m_SourceAssetExceptLabelFilterGUIDArray;
        /// <summary>
        /// 缓存收集规则类型
        /// </summary>
        private static Dictionary<string, Type> m_CacheFilterRuleTypes = new Dictionary<string, Type>();
        /// <summary>
        /// 缓存收集规则实例
        /// </summary>
        private static Dictionary<string, IFilterRule> m_CacheFilterRuleInstence = new Dictionary<string, IFilterRule>();

        /// <summary>
        /// 资源收集规则名称
        /// </summary>
        public static string[] FilterRules
        {
            get;
        }

        static ResourceCollectorEditorUtility()
        {
            m_CacheFilterRuleTypes.Clear();
            m_CacheFilterRuleInstence.Clear();
            List<Type> types = GameEditorUtility.GetAssignableTypes(typeof(IFilterRule));
            FilterRules = new string[types.Count];
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (m_CacheFilterRuleTypes.ContainsKey(type.Name))
                {
                    continue;
                }
                m_CacheFilterRuleTypes.Add(type.Name, type);
                FilterRules[i] = type.Name;
            }
        }

        public static void RefreshResourceCollection(ResourceGroupsCollector collectorData)
        {
            ResourceEditorController resourceEditorController = new();
            resourceEditorController.Load();

            collectorData ??= ResourcePackagesCollector.GetBundleCollectorByIndex(GameSetting.Instance.AssetBundleCollectorIndex);
            Debug.Log($"Export {collectorData.PackageName} ...");

            m_SourceAssetExceptTypeFilterGUIDArray = AssetDatabase.FindAssets(resourceEditorController.SourceAssetExceptTypeFilter);
            m_SourceAssetExceptLabelFilterGUIDArray = AssetDatabase.FindAssets(resourceEditorController.SourceAssetExceptLabelFilter);

            AnalysisResourceFilters(collectorData);

            if (SaveCollection())
            {
                Debug.Log("Refresh ResourceCollection.xml success");
            }
            else
            {
                Debug.Log("Refresh ResourceCollection.xml fail");
            }
        }

        public static IFilterRule GetFilterRuleInstance(string ruleName)
        {
            if (m_CacheFilterRuleInstence.TryGetValue(ruleName, out IFilterRule instance))
            {
                return instance;
            }

            if (m_CacheFilterRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IFilterRule)Activator.CreateInstance(type);
                m_CacheFilterRuleInstence.Add(ruleName, instance);
                return instance;
            }

            throw new Exception($"{nameof(IFilterRule)}类型无效:{ruleName}");
        }

        private static Resource[] GetResources()
        {
            return m_ResourceCollection.GetResources();
        }

        private static bool HasResource(string name, string Variant)
        {
            return m_ResourceCollection.HasResource(name, Variant);
        }

        private static bool AddResource(string name, string Variant, string FileSystem, LoadType LoadType, bool Packed, string[] resourceGroups)
        {
            return m_ResourceCollection.AddResource(name, Variant, FileSystem, LoadType, Packed, resourceGroups);
        }

        private static bool RenameResource(string oldName, string oldVariant, string newName, string newVariant)
        {
            return m_ResourceCollection.RenameResource(oldName, oldVariant, newName, newVariant);
        }

        private static bool AssignAsset(string assetGuid, string resourceName, string resourceVariant)
        {
            return m_ResourceCollection.AssignAsset(assetGuid, resourceName, resourceVariant);
        }

        private static void AnalysisResourceFilters(ResourceGroupsCollector collectorData)
        {
            m_ResourceCollection = new ResourceCollection();
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

                        FileInfo fileInfo = new(resourceCollector.AssetPath);
                        if (DefaultFilterRule.IsIgnoreFile(fileInfo.Extension))
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(resourceCollector.Variant))
                        {
                            resourceCollector.Variant = null;
                        }

                        if (AssetDatabase.IsValidFolder(resourceCollector.AssetPath) || File.Exists(resourceCollector.AssetPath))
                        {
                            string resourceName = string.IsNullOrEmpty(resourceCollector.Name) ? Path.GetFileNameWithoutExtension(fileInfo.FullName).ToLowerInvariant() : resourceCollector.Name.ToLowerInvariant();
                            ApplyResourceFilter(resourceCollector, resourceName, GetFilterRuleInstance(resourceCollector.FilterRule));
                        }
                        else
                        {
                            Debug.LogWarningFormat("assetPath {0} is invalid.", resourceCollector.AssetPath);
                        }
                    }
                }
            }
        }

        private static void ApplyResourceFilter(ResourceCollector assetCollector, string resourceName, IFilterRule filterRule)
        {
            foreach (var oldResource in GetResources())
            {
                if (oldResource.Name == resourceName && string.IsNullOrEmpty(oldResource.Variant))
                {
                    RenameResource(oldResource.Name, oldResource.Variant, resourceName, assetCollector.Variant);
                    break;
                }
            }

            if (!HasResource(resourceName, null))
            {
                if (string.IsNullOrEmpty(assetCollector.FileSystem))
                {
                    assetCollector.FileSystem = null;
                }

                AddResource(resourceName, assetCollector.Variant, assetCollector.FileSystem, assetCollector.LoadType, assetCollector.Packed, assetCollector.Groups.Split('|'));
            }

            if (AssetDatabase.IsValidFolder(assetCollector.AssetPath))
            {
                FileInfo[] assetFiles = new DirectoryInfo(assetCollector.AssetPath).GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in assetFiles)
                {
                    if (filterRule.IsCollectAsset(file.FullName))
                    {
                        string assetName = Path.Combine("Assets", file.FullName[(Application.dataPath.Length + 1)..]);
                        string assetGUID = AssetDatabase.AssetPathToGUID(assetName);
                        if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                        {
                            AssignAsset(assetGUID, resourceName, assetCollector.Variant);
                        }
                    }
                }
            }
            else
            {
                FileInfo file = new(assetCollector.AssetPath);
                if (filterRule.IsCollectAsset(file.FullName))
                {
                    string assetName = Path.Combine("Assets", file.FullName[(Application.dataPath.Length + 1)..]);
                    string assetGUID = AssetDatabase.AssetPathToGUID(assetName);
                    if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                    {
                        AssignAsset(assetGUID, resourceName, assetCollector.Variant);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("The resource collection rule is incorrect  AssetPath is {0},FilterRule is {1}", assetCollector.AssetPath, filterRule);
                }
            }
        }

        private static bool SaveCollection()
        {
            return m_ResourceCollection.Save();
        }
    }
}