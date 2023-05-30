﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityGameFramework.Editor.ResourceTools;
using GFResource = UnityGameFramework.Editor.ResourceTools.Resource;

namespace GeminiLion.Editor.ResourceTools
{
    /// <summary>
    /// Resource 规则编辑器，支持按规则配置自动生成 ResourceCollection.xml
    /// </summary>
    public class ResourceRuleEditor : EditorWindow
    {
        private readonly string m_NormalConfigurationPath = "Assets/GameMain/Scripts/Builtin/Editor/ResourceRuleEditor/ResourceRuleEditor.asset";
        private ResourceRuleEditorData m_Configuration;
        private ResourceCollection m_ResourceCollection;

        private ReorderableList m_RuleList;
        private Vector2 m_ScrollPosition = Vector2.zero;

        private string m_SourceAssetExceptTypeFilter = "t:Script";
        private string[] m_SourceAssetExceptTypeFilterGUIDArray;

        private string m_SourceAssetExceptLabelFilter = "l:ResourceExclusive";
        private string[] m_SourceAssetExceptLabelFilterGUIDArray;

        private int m_CurrentConfigIndex;
        [SerializeField] private string m_CurrentConfigPath;
        private List<string> m_AllConfigPaths;
        private string[] m_ConfigNames;

        private void OnGUI()
        {
            if (m_Configuration == null)
            {
                Load();
            }

            if (m_RuleList == null)
            {
                InitRuleListDrawer();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    Save();
                }

                if (GUILayout.Button("Refresh ResourceCollection.xml", EditorStyles.toolbarButton))
                {
                    RefreshResourceCollection();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                OnListElementLabelGUI();
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical();
            {
                GUILayout.Space(30);

                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    m_RuleList.DoLayoutList();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(m_Configuration);
            }
        }

        private void Load()
        {
            m_AllConfigPaths = AssetDatabase.FindAssets("t:ResourceRuleEditorData").Select(AssetDatabase.GUIDToAssetPath).ToList();
            m_ConfigNames = m_AllConfigPaths.Select(Path.GetFileNameWithoutExtension).ToArray();

            m_Configuration = LoadAssetAtPath<ResourceRuleEditorData>(m_CurrentConfigPath);
            if (m_Configuration == null)
            {
                if (m_AllConfigPaths.Count == 0)
                {
                    m_Configuration = ScriptableObject.CreateInstance<ResourceRuleEditorData>();
                    m_CurrentConfigPath = m_NormalConfigurationPath;
                    m_AllConfigPaths = new List<string>() { m_NormalConfigurationPath };
                    m_ConfigNames = new[] { Path.GetFileNameWithoutExtension(m_NormalConfigurationPath) };
                }
                else
                {
                    m_Configuration = LoadAssetAtPath<ResourceRuleEditorData>(m_AllConfigPaths[m_CurrentConfigIndex]);
                }

                m_CurrentConfigIndex = 0;
            }
            else
            {
                m_CurrentConfigIndex = m_AllConfigPaths.ToList().FindIndex(0, _ => string.Equals(m_CurrentConfigPath, _));
            }

            m_RuleList = null;
        }

        private T LoadAssetAtPath<T>(string path) where T : Object
        {
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }

        private void InitRuleListDrawer()
        {
            m_RuleList = new ReorderableList(m_Configuration.rules, typeof(ResourceRule));
            m_RuleList.drawElementCallback = OnListElementGUI;
            m_RuleList.drawHeaderCallback = OnListHeaderGUI;
            m_RuleList.draggable = true;
            m_RuleList.elementHeight = 22;
            m_RuleList.onAddCallback = (list) => Add();
        }

        private void Add()
        {
            m_Configuration.rules.Add(new ResourceRule());
        }

        private void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (index >= m_Configuration.rules.Count)
            {
                return;
            }

            const float GAP = 5;

            ResourceRule rule = m_Configuration.rules[index];
            rect.y++;

            Rect r = rect;
            r.width = 16;
            r.height = 18;
            rule.valid = EditorGUI.Toggle(r, rule.valid);
            GUI.enabled = rule.valid;

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 210;
            float assetBundleNameLength = r.width;
            rule.name = EditorGUI.TextField(r, rule.name);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 200;
            rule.loadType = (LoadType)EditorGUI.EnumPopup(r, rule.loadType);

            r.xMin = r.xMax + GAP + 15;
            r.xMax = r.xMin + 30;
            rule.packed = EditorGUI.Toggle(r, rule.packed);

            r.xMin = r.xMax + GAP + 5;
            r.xMax = r.xMin + 85;
            rule.fileSystem = EditorGUI.TextField(r, rule.fileSystem);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.groups = EditorGUI.TextField(r, rule.groups);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.variant = EditorGUI.TextField(r, rule.variant);
            if (!string.IsNullOrEmpty(rule.variant))
            {
                rule.variant = rule.variant.ToLower();
            }

            r.xMin = r.xMax + GAP;
            r.width = assetBundleNameLength + 230;
            GUI.enabled = rule.valid && !rule.lockPath;
            rule.assetsDirectoryPath = EditorGUI.TextField(r, rule.assetsDirectoryPath);
            GUI.enabled = rule.valid;
            if (DropPath(r, out string assetsPath))
            {
                rule.assetsDirectoryPath = assetsPath;
            }

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 30;
            rule.lockPath = EditorGUI.Toggle(r, rule.lockPath);

            r.xMin = r.xMax + GAP - 10;
            r.xMax = r.xMin + 30;
            if (GUI.Button(r, Directory.Exists(rule.assetsDirectoryPath) ? EditorGUIUtility.IconContent("Folder Icon") : EditorGUIUtility.IconContent("console.erroricon")))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rule.assetsDirectoryPath));
            }

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.filterType = (ResourceFilterType)EditorGUI.EnumPopup(r, rule.filterType);

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            rule.searchPatterns = EditorGUI.TextField(r, rule.searchPatterns);
            GUI.enabled = true;
        }

        private void OnListHeaderGUI(Rect rect)
        {
            Rect rules = new Rect(rect.x, rect.y, 100, rect.height);
            EditorGUI.LabelField(rules, "Rules");
            Rect configLabel = new Rect(rect.x + rules.width, rect.y, 90, rect.height);
            EditorGUI.LabelField(configLabel, "CurrentConfig:");
            Rect configs = new Rect(rect.x + rules.width + configLabel.width, rect.y, 200, rect.height);
            m_CurrentConfigIndex = EditorGUI.Popup(configs, m_CurrentConfigIndex, m_ConfigNames);
            if (m_CurrentConfigPath != m_AllConfigPaths[m_CurrentConfigIndex])
            {
                m_CurrentConfigPath = m_AllConfigPaths[m_CurrentConfigIndex];
                m_Configuration = LoadAssetAtPath<ResourceRuleEditorData>(m_CurrentConfigPath);
                m_RuleList = null;
            }

            Rect reload = new Rect(rect.width - 100, rect.y, 100, rect.height);
            if (GUI.Button(reload, "Reload"))
            {
                Load();
            }
        }

        private void OnListElementLabelGUI()
        {
            Rect rect = new Rect();
            const float GAP = 5;
            GUI.enabled = false;

            Rect r = new Rect(GAP, 20, rect.width, rect.height);
            r.width = 45;
            r.height = 18;
            EditorGUI.TextField(r, "Active");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 200;
            float assetBundleNameLength = r.width;
            EditorGUI.TextField(r, "Name");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 200;
            EditorGUI.TextField(r, "Load Type");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 50;
            EditorGUI.TextField(r, "Packed");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "File System");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Groups");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Variant");

            r.xMin = r.xMax + GAP;
            r.width = assetBundleNameLength + 300;
            EditorGUI.TextField(r, "AssetDirectory");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Filter Type");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 250;
            EditorGUI.TextField(r, "Patterns");
            GUI.enabled = true;
        }


        private bool DropPath(Rect dropArea, out string folderPath)
        {
            Event currentEvent = Event.current;
            folderPath = string.Empty;
            if (currentEvent.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                }
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        folderPath = AssetDatabase.GetAssetPath(draggedObject);
                        if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                        {
                            currentEvent.Use();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void Save()
        {
            if (LoadAssetAtPath<ResourceRuleEditorData>(m_CurrentConfigPath) == null)
            {
                AssetDatabase.CreateAsset(m_Configuration, m_CurrentConfigPath);
            }
            else
            {
                EditorUtility.SetDirty(m_Configuration);
            }
        }

        #region Refresh ResourceCollection.xml

        public void RefreshResourceCollection()
        {
            if (m_Configuration == null)
            {
                Load();
            }

            m_SourceAssetExceptTypeFilterGUIDArray = AssetDatabase.FindAssets(m_SourceAssetExceptTypeFilter);
            m_SourceAssetExceptLabelFilterGUIDArray = AssetDatabase.FindAssets(m_SourceAssetExceptLabelFilter);
            AnalysisResourceFilters();
            if (SaveCollection())
            {
                Debug.Log("Refresh ResourceCollection.xml success");
            }
            else
            {
                Debug.Log("Refresh ResourceCollection.xml fail");
            }
        }

        public void RefreshResourceCollection(string configPath)
        {
            if (m_Configuration == null || !m_CurrentConfigPath.Equals(configPath))
            {
                m_CurrentConfigPath = configPath;
                Load();
            }

            m_SourceAssetExceptTypeFilterGUIDArray = AssetDatabase.FindAssets(m_SourceAssetExceptTypeFilter);
            m_SourceAssetExceptLabelFilterGUIDArray = AssetDatabase.FindAssets(m_SourceAssetExceptLabelFilter);
            AnalysisResourceFilters();
            if (SaveCollection())
            {
                Debug.Log("Refresh ResourceCollection.xml success");
            }
            else
            {
                Debug.Log("Refresh ResourceCollection.xml fail");
            }
        }

        private GFResource[] GetResources()
        {
            return m_ResourceCollection.GetResources();
        }

        private bool HasResource(string name, string variant)
        {
            return m_ResourceCollection.HasResource(name, variant);
        }

        private bool AddResource(string name, string variant, string fileSystem,
            LoadType loadType, bool packed, string[] resourceGroups)
        {
            return m_ResourceCollection.AddResource(name, variant, fileSystem, loadType,
                packed, resourceGroups);
        }

        private bool RenameResource(string oldName, string oldVariant,
            string newName, string newVariant)
        {
            return m_ResourceCollection.RenameResource(oldName, oldVariant,
                newName, newVariant);
        }

        private bool AssignAsset(string assetGuid, string resourceName, string resourceVariant)
        {
            if (m_ResourceCollection.AssignAsset(assetGuid, resourceName, resourceVariant))
            {
                return true;
            }

            return false;
        }

        private void AnalysisResourceFilters()
        {
            m_ResourceCollection = new ResourceCollection();
            List<string> signedAssetBundleList = new List<string>();

            foreach (ResourceRule resourceRule in m_Configuration.rules)
            {
                if (resourceRule.variant == "")
                    resourceRule.variant = null;

                if (resourceRule.valid)
                {
                    switch (resourceRule.filterType)
                    {
                        case ResourceFilterType.Root:
                            {
                                if (string.IsNullOrEmpty(resourceRule.name))
                                {
                                    string relativeDirectoryName =
                                        resourceRule.assetsDirectoryPath.Replace("Assets/", "");
                                    ApplyResourceFilter(ref signedAssetBundleList, resourceRule,
                                        Utility.Path.GetRegularPath(relativeDirectoryName));
                                }
                                else
                                {
                                    ApplyResourceFilter(ref signedAssetBundleList, resourceRule,
                                        resourceRule.name);
                                }
                            }
                            break;

                        case ResourceFilterType.Children:
                            {
                                string[] patterns = resourceRule.searchPatterns.Split(';', ',', '|');
                                for (int i = 0; i < patterns.Length; i++)
                                {
                                    FileInfo[] assetFiles =
                                        new DirectoryInfo(resourceRule.assetsDirectoryPath).GetFiles(patterns[i],
                                            SearchOption.AllDirectories);
                                    foreach (FileInfo file in assetFiles)
                                    {
                                        if (file.Extension.Contains("meta"))
                                            continue;

                                        string relativeAssetName = file.FullName.Substring(Application.dataPath.Length + 1);
                                        string relativeAssetNameWithoutExtension =
                                            Utility.Path.GetRegularPath(
                                                relativeAssetName.Substring(0, relativeAssetName.LastIndexOf('.')));

                                        string assetName = Path.Combine("Assets", relativeAssetName);
                                        string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                        if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) &&
                                            !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                        {
                                            ApplyResourceFilter(ref signedAssetBundleList, resourceRule,
                                                relativeAssetNameWithoutExtension, assetGUID);
                                        }
                                    }
                                }
                            }
                            break;

                        case ResourceFilterType.ChildrenFoldersOnly:
                            {
                                DirectoryInfo[] assetDirectories =
                                    new DirectoryInfo(resourceRule.assetsDirectoryPath).GetDirectories();
                                foreach (DirectoryInfo directory in assetDirectories)
                                {
                                    string relativeDirectoryName =
                                        directory.FullName.Substring(Application.dataPath.Length + 1);

                                    ApplyResourceFilter(ref signedAssetBundleList, resourceRule,
                                        Utility.Path.GetRegularPath(relativeDirectoryName), string.Empty,
                                        directory.FullName);
                                }
                            }
                            break;

                        case ResourceFilterType.ChildrenFilesOnly:
                            {
                                DirectoryInfo[] assetDirectories =
                                    new DirectoryInfo(resourceRule.assetsDirectoryPath).GetDirectories();
                                foreach (DirectoryInfo directory in assetDirectories)
                                {
                                    string[] patterns = resourceRule.searchPatterns.Split(';', ',', '|');
                                    for (int i = 0; i < patterns.Length; i++)
                                    {
                                        FileInfo[] assetFiles =
                                            new DirectoryInfo(directory.FullName).GetFiles(patterns[i],
                                                SearchOption.AllDirectories);
                                        foreach (FileInfo file in assetFiles)
                                        {
                                            if (file.Extension.Contains("meta"))
                                                continue;

                                            string relativeAssetName =
                                                file.FullName.Substring(Application.dataPath.Length + 1);
                                            string relativeAssetNameWithoutExtension =
                                                Utility.Path.GetRegularPath(
                                                    relativeAssetName.Substring(0, relativeAssetName.LastIndexOf('.')));

                                            string assetName = Path.Combine("Assets", relativeAssetName);
                                            string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                            if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) &&
                                                !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                            {
                                                ApplyResourceFilter(ref signedAssetBundleList, resourceRule,
                                                    relativeAssetNameWithoutExtension, assetGUID);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void ApplyResourceFilter(ref List<string> signedResourceList, ResourceRule resourceRule,
            string resourceName, string singleAssetGUID = "", string childDirectoryPath = "")
        {
            if (!signedResourceList.Contains(Path.Combine(resourceRule.assetsDirectoryPath, resourceName)))
            {
                signedResourceList.Add(Path.Combine(resourceRule.assetsDirectoryPath, resourceName));

                foreach (GFResource oldResource in GetResources())
                {
                    if (oldResource.Name == resourceName && string.IsNullOrEmpty(oldResource.Variant))
                    {
                        RenameResource(oldResource.Name, oldResource.Variant,
                            resourceName, resourceRule.variant);
                        break;
                    }
                }

                if (!HasResource(resourceName, resourceRule.variant))
                {
                    if (string.IsNullOrEmpty(resourceRule.fileSystem))
                    {
                        resourceRule.fileSystem = null;
                    }

                    AddResource(resourceName, resourceRule.variant, resourceRule.fileSystem,
                        resourceRule.loadType, resourceRule.packed,
                        resourceRule.groups.Split(';', ',', '|'));
                }

                switch (resourceRule.filterType)
                {
                    case ResourceFilterType.Root:
                    case ResourceFilterType.ChildrenFoldersOnly:
                        string[] patterns = resourceRule.searchPatterns.Split(';', ',', '|');
                        if (childDirectoryPath == "")
                        {
                            childDirectoryPath = resourceRule.assetsDirectoryPath;
                        }

                        for (int i = 0; i < patterns.Length; i++)
                        {
                            FileInfo[] assetFiles =
                                new DirectoryInfo(childDirectoryPath).GetFiles(patterns[i],
                                    SearchOption.AllDirectories);
                            foreach (FileInfo file in assetFiles)
                            {
                                if (file.Extension.Contains("meta"))
                                    continue;

                                string assetName = Path.Combine("Assets",
                                    file.FullName.Substring(Application.dataPath.Length + 1));

                                string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) &&
                                    !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                {
                                    AssignAsset(assetGUID, resourceName,
                                        resourceRule.variant);
                                }
                            }
                        }

                        break;

                    case ResourceFilterType.Children:
                    case ResourceFilterType.ChildrenFilesOnly:
                        {
                            AssignAsset(singleAssetGUID, resourceName,
                                resourceRule.variant);
                        }
                        break;
                }
            }
        }

        private bool SaveCollection()
        {
            return m_ResourceCollection.Save();
        }

        #endregion
    }
}