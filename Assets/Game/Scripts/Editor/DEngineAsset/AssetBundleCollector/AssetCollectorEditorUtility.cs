using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using Game.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    internal static class AssetCollectorEditorUtility
    {
        // 资源规则
        private static ResourceCollection m_ResourceCollection;
        // 资源筛选控制
        private static ResourceEditorController m_ResourceEditorController;
        // 排除的类型
        private static string[] m_SourceAssetExceptTypeFilterGUIDArray;
        // 排除的标签
        private static string[] m_SourceAssetExceptLabelFilterGUIDArray;
        /// <summary>
        /// 资源收集规则
        /// </summary>
        public static string[] FilterRules { get; }

        static AssetCollectorEditorUtility()
        {
            FilterRules = GameEditorUtility.GetAssignableTypes(typeof(IFilterRule)).Select(x => x.Name).ToArray();
        }

        public static void RefreshResourceCollection(AssetBundleCollector collectorData)
        {
            if (m_ResourceEditorController == null)
            {
                m_ResourceEditorController = new ResourceEditorController();
                m_ResourceEditorController.Load();
            }

            collectorData ??= AssetBundlePackageCollector.GetBundleCollectorByIndex(GameSetting.Instance.AssetBundleCollectorIndex);

            m_SourceAssetExceptTypeFilterGUIDArray = AssetDatabase.FindAssets(m_ResourceEditorController.SourceAssetExceptTypeFilter);
            m_SourceAssetExceptLabelFilterGUIDArray = AssetDatabase.FindAssets(m_ResourceEditorController.SourceAssetExceptLabelFilter);

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

        private static void AnalysisResourceFilters(AssetBundleCollector collectorData)
        {
            m_ResourceCollection = new ResourceCollection();
            List<string> signedAssetBundleList = new List<string>();
            List<Type> filteRuleType = GameEditorUtility.GetAssignableTypes(typeof(IFilterRule));
            for (int x = 0; x < collectorData.Groups.Count; x++)
            {
                var resourceGroup = collectorData.Groups[x];
                if (resourceGroup.EnableGroup)
                {
                    for (int y = 0; y < resourceGroup.AssetCollectors.Count; y++)
                    {
                        var resourceCollector = resourceGroup.AssetCollectors[y];
                        if (!resourceCollector.Enable)
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(resourceCollector.Variant))
                        {
                            resourceCollector.Variant = null;
                        }

                        string fileRuleName = FilterRules[resourceCollector.FilterRule];
                        Type filterRuleType = filteRuleType.Find(x => x.Name == fileRuleName);
                        if (filterRuleType != null)
                        {
                            IFilterRule filterRule = (IFilterRule)Activator.CreateInstance(filterRuleType);
                            if (filterRule != null)
                            {
                                if (AssetDatabase.IsValidFolder(resourceCollector.AssetPath) || File.Exists(resourceCollector.AssetPath))
                                {
                                    string resourceName;
                                    if (string.IsNullOrEmpty(resourceCollector.Name))
                                    {
                                        resourceName = Path.GetFileNameWithoutExtension(resourceCollector.AssetPath).ToLower();
                                    }
                                    else
                                    {
                                        resourceName = resourceCollector.Name;
                                    }

                                    ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, resourceName, filterRule);
                                }
                                else
                                {
                                    Debug.LogWarningFormat("assetPath {0} is invalid.", resourceCollector.AssetPath);
                                }

                            }
                        }
                    }
                }
            }
        }

        private static void ApplyResourceFilter(ref List<string> signedResourceList, AssetCollector assetCollector, string resourceName, IFilterRule filterRule)
        {
            if (!signedResourceList.Contains(Path.Combine(assetCollector.AssetPath, resourceName)))
            {
                signedResourceList.Add(Path.Combine(assetCollector.AssetPath, resourceName));

                foreach (var oldResource in GetResources())
                {
                    if (oldResource.Name == resourceName && string.IsNullOrEmpty(oldResource.Variant))
                    {
                        RenameResource(oldResource.Name, oldResource.Variant, resourceName, null);
                        break;
                    }
                }

                if (!HasResource(resourceName, null))
                {
                    if (string.IsNullOrEmpty(assetCollector.FileSystem))
                    {
                        assetCollector.FileSystem = null;
                    }

                    AddResource(resourceName, null, assetCollector.FileSystem, assetCollector.LoadType, assetCollector.Packed, assetCollector.Groups.Split(';', ',', '|'));
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
                                AssignAsset(assetGUID, resourceName, null);
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
                            AssignAsset(assetGUID, resourceName, null);
                        }
                    }
                }
            }
        }

        private static bool SaveCollection()
        {
            return m_ResourceCollection.Save();
        }
    }
}