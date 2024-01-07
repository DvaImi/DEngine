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
        // 文件搜索模式
        private const string m_SearchPattern = "*.*";
        // 根路径
        private static string m_RootPath;

        public static void RefreshResourceCollection(AssetBundleCollector collectorData = null)
        {
            if (m_ResourceEditorController == null)
            {
                m_ResourceEditorController = new ResourceEditorController();
                m_ResourceEditorController.Load();
            }
            if (collectorData == null)
            {
                collectorData = AssetBundleCollector.Load();
            }

            m_RootPath = Utility.Text.Format("{0}/{1}", Application.dataPath, m_ResourceEditorController.SourceAssetRootPath.Replace("Assets/", string.Empty));

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

                        switch (resourceCollector.FilterType)
                        {
                            case FilterType.Root:
                                {
                                    if (string.IsNullOrEmpty(resourceCollector.Name))
                                    {
                                        string relativeDirectoryName = resourceCollector.AssetPath.Replace(m_ResourceEditorController.SourceAssetRootPath + "/", "");
                                        ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, Utility.Path.GetRegularPath(relativeDirectoryName));
                                    }
                                    else
                                    {
                                        ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, resourceCollector.Name);
                                    }
                                }
                                break;

                            case FilterType.Children:
                                {
                                    FileInfo[] assetFiles = new DirectoryInfo(resourceCollector.AssetPath).GetFiles(m_SearchPattern, SearchOption.AllDirectories);
                                    foreach (FileInfo file in assetFiles)
                                    {
                                        if (file.Extension.Contains("meta"))
                                        {
                                            continue;
                                        }

                                        string relativeAssetName = file.FullName.Substring(m_RootPath.Length + 1);
                                        string relativeAssetNameWithoutExtension = Utility.Path.GetRegularPath(relativeAssetName.Substring(0, relativeAssetName.LastIndexOf('.')));

                                        string assetName = Path.Combine(m_ResourceEditorController.SourceAssetRootPath, relativeAssetName);
                                        string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                        if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                        {
                                            ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, relativeAssetNameWithoutExtension, assetGUID);
                                        }
                                    }
                                }
                                break;

                            case FilterType.ChildrenFoldersOnly:
                                {
                                    DirectoryInfo[] assetDirectories = new DirectoryInfo(resourceCollector.AssetPath).GetDirectories();
                                    foreach (DirectoryInfo directory in assetDirectories)
                                    {
                                        if (string.IsNullOrEmpty(resourceCollector.Name))
                                        {
                                            string relativeDirectoryName = directory.FullName.Substring(m_RootPath.Length + 1);
                                            ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, Utility.Path.GetRegularPath(relativeDirectoryName), string.Empty, directory.FullName);
                                        }
                                        else
                                        {
                                            ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, resourceCollector.Name, string.Empty, directory.FullName);
                                        }
                                    }
                                }
                                break;

                            case FilterType.ChildrenFilesOnly:
                                {
                                    DirectoryInfo[] assetDirectories = new DirectoryInfo(resourceCollector.AssetPath).GetDirectories();
                                    foreach (DirectoryInfo directory in assetDirectories)
                                    {
                                        FileInfo[] assetFiles = new DirectoryInfo(directory.FullName).GetFiles(m_SearchPattern, SearchOption.AllDirectories);
                                        foreach (FileInfo file in assetFiles)
                                        {
                                            if (file.Extension.Contains("meta"))
                                            {
                                                continue;
                                            }
                                            string relativeAssetName =
                                            file.FullName.Substring(m_RootPath.Length + 1);
                                            string relativeAssetNameWithoutExtension = Utility.Path.GetRegularPath(relativeAssetName.Substring(0, relativeAssetName.LastIndexOf('.')));

                                            string assetName = Path.Combine(m_ResourceEditorController.SourceAssetRootPath, relativeAssetName);
                                            string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                            if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                            {
                                                ApplyResourceFilter(ref signedAssetBundleList, resourceCollector, relativeAssetNameWithoutExtension, assetGUID);
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static void ApplyResourceFilter(ref List<string> signedResourceList, AssetCollector assetCollector, string resourceName, string singleAssetGUID = "", string childDirectoryPath = "")
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

                switch (assetCollector.FilterType)
                {
                    case FilterType.Root:
                    case FilterType.ChildrenFoldersOnly:
                        string[] patterns = assetCollector.SearchPatterns.Split(';', ',', '|');
                        if (childDirectoryPath == "")
                        {
                            childDirectoryPath = assetCollector.AssetPath;
                        }

                        for (int i = 0; i < patterns.Length; i++)
                        {
                            FileInfo[] assetFiles = new DirectoryInfo(childDirectoryPath).GetFiles(patterns[i], SearchOption.AllDirectories);
                            foreach (FileInfo file in assetFiles)
                            {
                                if (file.Extension.Contains("meta"))
                                {
                                    continue;
                                }

                                string assetName = Path.Combine("Assets",
                                file.FullName[(Application.dataPath.Length + 1)..]);

                                string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                {
                                    AssignAsset(assetGUID, resourceName, null);
                                }
                            }
                        }

                        break;

                    case FilterType.Children:
                    case FilterType.ChildrenFilesOnly:
                        {
                            AssignAsset(singleAssetGUID, resourceName, null);
                        }
                        break;
                }
            }
        }

        private static bool SaveCollection()
        {
            return m_ResourceCollection.Save();
        }
    }
}