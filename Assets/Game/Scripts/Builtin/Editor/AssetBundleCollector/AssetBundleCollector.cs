using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using GFResource = UnityGameFramework.Editor.ResourceTools.Resource;

namespace Game.Editor.ResourceTools
{
    /// <summary>
    /// Resource 规则编辑器，支持按规则配置自动生成 ResourceCollection.xml
    /// </summary>
    public class AssetBundleCollector : EditorWindow
    {
        private readonly string m_NormalConfigurationPath = "Assets/Game/Scripts/Builtin/Editor/AssetBundleCollector/Configs";
        private readonly string m_NormalFileName = "AssetBundleCollector{0}.asset";
        private AssetBundleData m_Configuration;
        private ResourceCollection m_ResourceCollection;

        private ReorderableList m_RuleList;
        private Vector2 m_ScrollPosition = Vector2.zero;

        private string m_SourceAssetExceptTypeFilter = "t:Script";
        private string[] m_SourceAssetExceptTypeFilterGUIDArray;

        private string m_SourceAssetExceptLabelFilter = "l:ResourceExclusive";
        private string[] m_SourceAssetExceptLabelFilterGUIDArray;

        private int m_CurrentConfigIndex;
        private string m_CurrentConfigPath;
        private List<string> m_AllConfigPaths;
        private string[] m_ConfigNames;
        private bool m_build = false;


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

        private void Update()
        {
            if (m_build)
            {
                m_build = false;
                AssetBundleUtility.StartBuild();
            }
        }

        private void Load()
        {
            m_AllConfigPaths = AssetDatabase.FindAssets("t:AssetBundleData").Select(AssetDatabase.GUIDToAssetPath).ToList();
            m_ConfigNames = m_AllConfigPaths.Select(Path.GetFileNameWithoutExtension).ToArray();
            m_Configuration = LoadAssetAtPath<AssetBundleData>(m_CurrentConfigPath);
            if (m_Configuration == null)
            {
                if (m_AllConfigPaths.Count == 0)
                {
                    m_Configuration = CreateInstance<AssetBundleData>();
                    m_CurrentConfigPath = Path.Combine(m_NormalConfigurationPath, string.Format(m_NormalFileName, ""));
                    m_AllConfigPaths = new List<string>() { m_CurrentConfigPath };
                    m_ConfigNames = new[] { Path.GetFileNameWithoutExtension(string.Format(m_NormalFileName, "")) };
                }
                else
                {
                    m_Configuration = LoadAssetAtPath<AssetBundleData>(m_AllConfigPaths[m_CurrentConfigIndex]);
                }
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
            m_RuleList.onSelectCallback = Select;
        }

        private void Add()
        {
            m_Configuration.rules.Add(new ResourceRule());
        }

        private void Select(ReorderableList list)
        {
            int index = list.index;
            if (m_Configuration == null)
            {
                Load();
            }

            string select = m_Configuration.rules[index].assetPath;
            Object obj = AssetDatabase.LoadMainAssetAtPath(select);
            Selection.activeObject = obj;
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
            r.width = assetBundleNameLength + 270;
            rule.assetPath = EditorGUI.TextField(r, rule.assetPath);

            if (DropPath(r, out string assetsPath, rule.filterType == ResourceFilterType.FileOnly))
            {
                rule.assetPath = assetsPath;
            }

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 30;
            bool isFile = rule.filterType == ResourceFilterType.FileOnly;
            if (GUI.Button(r, (isFile ? File.Exists(rule.assetPath) : Directory.Exists(rule.assetPath)) ? EditorGUIUtility.IconContent("Folder Icon") : EditorGUIUtility.IconContent("console.erroricon"), new GUIStyle() { imagePosition = ImagePosition.ImageAbove }))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rule.assetPath));
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
            if (m_CurrentConfigIndex < 0 || m_CurrentConfigIndex > m_AllConfigPaths.Count)
            {
                m_Configuration = null;
                return;
            }
            if (m_CurrentConfigPath != m_AllConfigPaths[m_CurrentConfigIndex])
            {
                m_CurrentConfigPath = m_AllConfigPaths[m_CurrentConfigIndex];
                m_Configuration = LoadAssetAtPath<AssetBundleData>(m_CurrentConfigPath);
                m_RuleList = null;
            }

            Rect newRule = new Rect(configs.x + configs.width + 5, configs.y, 100, configs.height);
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 20;
            if (GUI.Button(newRule, "+", style))
            {
                string format = "(" + m_AllConfigPaths.Count + ")";
                string newPath = Path.Combine(m_NormalConfigurationPath, string.Format(m_NormalFileName, format));
                if (File.Exists(newPath))
                {
                    string format2 = "(" + (m_AllConfigPaths.Count + 1) + ")";
                    newPath = newPath.Replace(format, format2);
                }
                m_CurrentConfigPath = Utility.Path.GetRegularPath(newPath);

                AssetDatabase.CreateAsset(CreateInstance<AssetBundleData>(), newPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Load();
            }

            Rect delete = new Rect(newRule.x + newRule.width + 5, newRule.y, 100, newRule.height);
            GUI.enabled = m_AllConfigPaths.Count > 1;
            if (GUI.Button(delete, "X"))
            {
                int deleteindex = m_CurrentConfigIndex;
                AssetDatabase.DeleteAsset(m_AllConfigPaths[deleteindex]);
                if (deleteindex == 0)
                {
                    m_CurrentConfigIndex = deleteindex;
                }
                else
                {
                    m_CurrentConfigIndex = deleteindex - 1;
                }
                m_RuleList = null;
                Load();
            }
            GUI.enabled = true;
            Rect save = new Rect(delete.x + delete.width + 5, delete.y, 100, delete.height);

            if (GUI.Button(save, EditorGUIUtility.IconContent("Save")))
            {
                Save();
                RefreshResourceCollection();
                AssetDatabase.SaveAssets();
            }

            Rect reload = new Rect(save.x + save.width + 5, save.y, 100, save.height);
            if (GUI.Button(reload, EditorGUIUtility.IconContent("Refresh")))
            {
                Load();
            }

            Rect address = new Rect(rect.xMax - 250, reload.y, 120, reload.height);
            m_Configuration.EnableAddress = GUI.Toggle(address, m_Configuration.EnableAddress, "可寻址加载");

            Rect build = new Rect(rect.xMax - 130, reload.y, 120, reload.height);
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUI.Button(build, "Build"))
            {
                m_build = true;
            }
            GUI.backgroundColor = bc;
        }

        private void OnListElementLabelGUI()
        {
            Rect rect = new Rect();
            const float GAP = 5;
            GUI.enabled = false;

            Rect r = new Rect(GAP, GAP, rect.width, rect.height);
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
            EditorGUI.TextField(r, "AssetPath");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            EditorGUI.TextField(r, "Filter Type");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 250;
            EditorGUI.TextField(r, "Patterns");
            GUI.enabled = true;
        }

        private bool DropPath(Rect dropArea, out string assetPath, bool files = false)
        {
            Event currentEvent = Event.current;
            assetPath = string.Empty;
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
                        assetPath = AssetDatabase.GetAssetPath(draggedObject);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            currentEvent.Use();
                            return files ? File.Exists(assetPath) : Directory.Exists(assetPath);
                        }
                    }
                }
            }
            return false;
        }

        private void Save()
        {
            if (LoadAssetAtPath<AssetBundleData>(m_CurrentConfigPath) == null)
            {
                AssetDatabase.CreateAsset(m_Configuration, m_CurrentConfigPath);
            }
            else
            {
                EditorUtility.SetDirty(m_Configuration);
            }
        }

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

        public bool EnableAddress()
        {
            if (m_Configuration == null)
            {
                Load();
            }

            return m_Configuration.EnableAddress;
        }

        private GFResource[] GetResources()
        {
            return m_ResourceCollection.GetResources();
        }

        private bool HasResource(string name, string variant)
        {
            return m_ResourceCollection.HasResource(name, variant);
        }

        private bool AddResource(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            return m_ResourceCollection.AddResource(name, variant, fileSystem, loadType,
                packed, resourceGroups);
        }

        private bool RenameResource(string oldName, string oldVariant, string newName, string newVariant)
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
                                        resourceRule.assetPath.Replace("Assets/", "");
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
                                        new DirectoryInfo(resourceRule.assetPath).GetFiles(patterns[i],
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
                                    new DirectoryInfo(resourceRule.assetPath).GetDirectories();
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
                                    new DirectoryInfo(resourceRule.assetPath).GetDirectories();
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
                        case ResourceFilterType.FileOnly:
                            FileInfo assetFile = new FileInfo(resourceRule.assetPath);
                            string assetFileName = assetFile.FullName.Substring(Application.dataPath.Length + 1);
                            string assetNameWithoutExtension = Utility.Path.GetRegularPath(assetFileName.Substring(0, assetFileName.LastIndexOf('.')));
                            assetFileName = Path.Combine("Assets", assetFileName);
                            string assetFileGUID = AssetDatabase.AssetPathToGUID(assetFileName);

                            if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetFileGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetFileGUID))
                            {
                                ApplyResourceFilter(ref signedAssetBundleList, resourceRule, assetNameWithoutExtension, assetFileGUID);
                            }
                            break;
                    }
                }
            }
        }

        private void ApplyResourceFilter(ref List<string> signedResourceList, ResourceRule resourceRule, string resourceName, string singleAssetGUID = "", string childDirectoryPath = "")
        {
            if (!signedResourceList.Contains(Path.Combine(resourceRule.assetPath, resourceName)))
            {
                signedResourceList.Add(Path.Combine(resourceRule.assetPath, resourceName));

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
                            childDirectoryPath = resourceRule.assetPath;
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
                    case ResourceFilterType.FileOnly:
                        {
                            AssignAsset(singleAssetGUID, resourceName, resourceRule.variant);
                        }
                        break;
                }
            }
        }

        private bool SaveCollection()
        {
            return m_ResourceCollection.Save();
        }
    }
}