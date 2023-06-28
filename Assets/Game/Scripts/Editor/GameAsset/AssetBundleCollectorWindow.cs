using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine.Editor.ResourceTools;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using DEResource = DEngine.Editor.ResourceTools.Resource;

namespace Game.Editor.ResourceTools
{
    /// <summary>
    /// 资源收集器窗口
    /// </summary>
    public class AssetBundleCollectorWindow : EditorWindow
    {
        private readonly string m_NormalConfigurationPath = "Assets/Game/Scripts/Builtin/Editor/GameAsset/Configs";
        private readonly string m_NormalFileName = "AssetBundleCollector{0}.asset";
        private AssetBundleCollector m_Configuration;
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
        private SerializedObject m_SerializedObject;

        [MenuItem("Game/AssetCollector", false, 1)]
        static void Open()
        {
            EditorWindow window = GetWindow<AssetBundleCollectorWindow>(false, "AssetBundleCollector");
            window.minSize = new Vector2(1640f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);
            Load();

            m_RuleList = new ReorderableList(m_Configuration.Collector, typeof(AssetCollector))
            {
                drawElementCallback = OnListElementGUI,
                drawHeaderCallback = DrawReorderableListHeader,
                draggable = true,
                elementHeight = 22,
                onAddCallback = Add,
                onSelectCallback = Select
            };
        }

        private void OnGUI()
        {
            m_SerializedObject.Update();

            GUILayout.BeginHorizontal();
            {
                DrawElementLabelGUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                DrawListHeaderGUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginVertical();
            {
                GUILayout.Space(5);
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
                {
                    GUILayout.BeginHorizontal();
                    {
                        m_RuleList.DoLayoutList();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(m_Configuration);
            }

            m_SerializedObject.ApplyModifiedProperties();
        }

        private void Load()
        {
            m_AllConfigPaths = AssetDatabase.FindAssets("t:AssetBundleCollector").Select(AssetDatabase.GUIDToAssetPath).ToList();
            m_ConfigNames = m_AllConfigPaths.Select(Path.GetFileNameWithoutExtension).ToArray();
            m_Configuration = LoadAssetAtPath<AssetBundleCollector>(m_CurrentConfigPath);
            if (m_Configuration == null)
            {
                if (m_AllConfigPaths.Count == 0)
                {
                    m_Configuration = CreateInstance<AssetBundleCollector>();
                    m_CurrentConfigPath = Path.Combine(m_NormalConfigurationPath, string.Format(m_NormalFileName, ""));
                    m_AllConfigPaths = new List<string>() { m_CurrentConfigPath };
                    m_ConfigNames = new[] { Path.GetFileNameWithoutExtension(string.Format(m_NormalFileName, "")) };
                }
                else
                {
                    m_Configuration = LoadAssetAtPath<AssetBundleCollector>(m_AllConfigPaths[m_CurrentConfigIndex]);
                }
            }
            else
            {
                m_CurrentConfigIndex = m_AllConfigPaths.ToList().FindIndex(0, _ => string.Equals(m_CurrentConfigPath, _));
            }
        }

        private T LoadAssetAtPath<T>(string path) where T : Object
        {
            return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
        }

        private void Add(ReorderableList list)
        {
            m_Configuration.Collector.Add(new AssetCollector());
        }

        private void Select(ReorderableList list)
        {
            int index = list.index;
            if (m_Configuration == null)
            {
                Load();
            }

            var select = m_Configuration.Collector[index];
            if (AssetPathvalid(select))
            {
                Object obj = AssetDatabase.LoadMainAssetAtPath(select.assetPath);
                Selection.activeObject = obj;
            }
        }

        private void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (index >= m_Configuration.Collector.Count)
            {
                return;
            }

            const float GAP = 5;

            AssetCollector rule = m_Configuration.Collector[index];
            bool valid = AssetPathvalid(rule);
            rect.y++;

            Rect r = rect;
            r.width = 16;
            r.height = 18;
            rule.valid = EditorGUI.Toggle(r, rule.valid);
            GUI.enabled = rule.valid;

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 210;
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
            r.xMax = rect.xMax - 420;
            Color bc = GUI.contentColor;
            if (!valid)
            {
                GUI.contentColor = new Color(1, 0.3F, 0.3F, 1);
            }

            rule.assetPath = EditorGUI.TextField(r, rule.assetPath);
            GUI.contentColor = bc;

            if (PathUtility.DropPathOutType(r, out string assetsPath, out bool isFile))
            {
                rule.assetPath = assetsPath;
                rule.filterType = isFile ? FilterType.FileOnly : rule.filterType;
            }

            r.xMin = r.xMax + GAP - 20;
            r.xMax = r.xMin + 25;
            GUI.enabled = valid;
            if (GUI.Button(r, valid ? EditorGUIUtility.IconContent("pick") : EditorGUIUtility.IconContent("console.erroricon")))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(rule.assetPath));
            }
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 60;
            if (GUI.Button(r, "Browse"))
            {
                string newAssetPath = rule.filterType == FilterType.FileOnly
                    ? EditorUtility.OpenFilePanel("Select AssetPath Folder", rule.assetPath, rule.searchPatterns)
                    : EditorUtility.OpenFolderPanel("Select AssetPath Folder", rule.assetPath, string.Empty);
                rule.assetPath = PathUtility.ConvertToAssetPath(newAssetPath);
            }

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMin + 85;
            rule.filterType = (FilterType)EditorGUI.EnumPopup(r, rule.filterType);

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            rule.searchPatterns = EditorGUI.TextField(r, rule.searchPatterns);
            GUI.enabled = true;

        }

        private void DrawListHeaderGUI()
        {
            EditorGUILayout.LabelField("Collector", GUILayout.Width(80));
            EditorGUILayout.LabelField("CurrentConfig:", GUILayout.Width(100));
            m_CurrentConfigIndex = EditorGUILayout.Popup(m_CurrentConfigIndex, m_ConfigNames, GUILayout.Width(200));
            if (m_CurrentConfigIndex < 0 || m_CurrentConfigIndex > m_AllConfigPaths.Count)
            {
                m_Configuration = null;
                return;
            }
            if (m_CurrentConfigPath != m_AllConfigPaths[m_CurrentConfigIndex])
            {
                m_CurrentConfigPath = m_AllConfigPaths[m_CurrentConfigIndex];
                m_Configuration = LoadAssetAtPath<AssetBundleCollector>(m_CurrentConfigPath);
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("pick"), GUILayout.Width(30), GUILayout.Height(20)))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(m_CurrentConfigPath));
            }

            GUIStyle style = new(GUI.skin.button)
            {
                fontSize = 20
            };

            if (GUILayout.Button("+", style, GUILayout.Width(40), GUILayout.Height(20)))
            {
                string format = "(" + m_AllConfigPaths.Count + ")";
                string newPath = Path.Combine(m_NormalConfigurationPath, string.Format(m_NormalFileName, format));
                if (File.Exists(newPath))
                {
                    string format2 = "(" + (m_AllConfigPaths.Count + 1) + ")";
                    newPath = newPath.Replace(format, format2);
                }
                m_CurrentConfigPath = DEngine.Utility.Path.GetRegularPath(newPath);

                AssetDatabase.CreateAsset(CreateInstance<AssetBundleCollector>(), newPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Load();
            }

            GUI.enabled = m_AllConfigPaths.Count > 1;
            if (GUILayout.Button("X", GUILayout.Width(40), GUILayout.Height(20)))
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
                Load();
            }

            GUI.enabled = true;

            GUILayout.Space(20);

            GUILayout.FlexibleSpace();
            Color bc = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(EditorGUIUtility.IconContent("Save"), GUILayout.Width(100)))
            {
                Save();
                RefreshResourceCollection();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(100)))
            {
                Load();
            }
            GUI.backgroundColor = bc;
        }

        private void DrawReorderableListHeader(Rect rect)
        {

        }

        private void DrawElementLabelGUI()
        {
            const float GAP = 2;
            GUI.enabled = false;
            EditorGUILayout.TextField("Active", GUILayout.Width(45), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Name", GUILayout.Width(200), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Load Type", GUILayout.Width(200), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Packed", GUILayout.Width(50), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("File System", GUILayout.Width(85), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Groups", GUILayout.Width(85), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Variant", GUILayout.Width(85), GUILayout.Height(20));
            GUILayout.Space(GAP);
            GUILayoutOption[] textFieldOptions = { GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - (2 * GAP)) };
            EditorGUILayout.TextField("AssetPath", textFieldOptions);
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Filter Type", GUILayout.Width(85), GUILayout.Height(20));
            GUILayout.Space(GAP);
            EditorGUILayout.TextField("Patterns", GUILayout.Width(250), GUILayout.Height(20));
            GUI.enabled = true;
        }

        private void Save()
        {
            if (LoadAssetAtPath<AssetBundleCollector>(m_CurrentConfigPath) == null)
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

        private bool AssetPathvalid(AssetCollector resourceRule)
        {
            return resourceRule != null && (resourceRule.filterType == FilterType.FileOnly ? File.Exists(resourceRule.assetPath) : Directory.Exists(resourceRule.assetPath));
        }

        private DEResource[] GetResources()
        {
            return m_ResourceCollection.GetResources();
        }

        private bool HasResource(string name, string variant)
        {
            return m_ResourceCollection.HasResource(name, variant);
        }

        private bool AddResource(string name, string variant, string fileSystem, LoadType loadType, bool packed, string[] resourceGroups)
        {
            return m_ResourceCollection.AddResource(name, variant, fileSystem, loadType, packed, resourceGroups);
        }

        private bool RenameResource(string oldName, string oldVariant, string newName, string newVariant)
        {
            return m_ResourceCollection.RenameResource(oldName, oldVariant, newName, newVariant);
        }

        private bool AssignAsset(string assetGuid, string resourceName, string resourceVariant)
        {
            return m_ResourceCollection.AssignAsset(assetGuid, resourceName, resourceVariant);
        }

        private void AnalysisResourceFilters()
        {
            m_ResourceCollection = new ResourceCollection();
            List<string> signedAssetBundleList = new List<string>();

            foreach (AssetCollector assetCollector in m_Configuration.Collector)
            {
                if (assetCollector.variant == "")
                {
                    assetCollector.variant = null;
                }

                if (assetCollector.valid)
                {
                    if (!AssetPathvalid(assetCollector))
                    {
                        Debug.LogWarning($"AssetPath [{assetCollector.assetPath}] is invalid");
                        return;
                    }
                    switch (assetCollector.filterType)
                    {
                        case FilterType.Root:
                            {
                                if (string.IsNullOrEmpty(assetCollector.name))
                                {
                                    string relativeDirectoryName = assetCollector.assetPath.Replace("Assets/", "");
                                    ApplyResourceFilter(ref signedAssetBundleList, assetCollector, DEngine.Utility.Path.GetRegularPath(relativeDirectoryName));
                                }
                                else
                                {
                                    ApplyResourceFilter(ref signedAssetBundleList, assetCollector, assetCollector.name);
                                }
                            }
                            break;

                        case FilterType.Children:
                            {
                                string[] patterns = assetCollector.searchPatterns.Split(';', ',', '|');
                                for (int i = 0; i < patterns.Length; i++)
                                {
                                    FileInfo[] assetFiles = new DirectoryInfo(assetCollector.assetPath).GetFiles(patterns[i], SearchOption.AllDirectories);
                                    foreach (FileInfo file in assetFiles)
                                    {
                                        if (file.Extension.Contains("meta"))
                                        {
                                            continue;
                                        }

                                        string relativeAssetName = file.FullName[(Application.dataPath.Length + 1)..];
                                        string relativeAssetNameWithoutExtension = DEngine.Utility.Path.GetRegularPath(relativeAssetName[..relativeAssetName.LastIndexOf('.')]);

                                        string assetName = Path.Combine("Assets", relativeAssetName);
                                        string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                        if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                        {
                                            ApplyResourceFilter(ref signedAssetBundleList, assetCollector, relativeAssetNameWithoutExtension, assetGUID);
                                        }
                                    }
                                }
                            }
                            break;

                        case FilterType.ChildrenFoldersOnly:
                            {
                                DirectoryInfo[] assetDirectories = new DirectoryInfo(assetCollector.assetPath).GetDirectories();
                                foreach (DirectoryInfo directory in assetDirectories)
                                {
                                    string relativeDirectoryName = directory.FullName[(Application.dataPath.Length + 1)..];

                                    ApplyResourceFilter(ref signedAssetBundleList, assetCollector, DEngine.Utility.Path.GetRegularPath(relativeDirectoryName), string.Empty, directory.FullName);
                                }
                            }
                            break;

                        case FilterType.ChildrenFilesOnly:
                            {
                                DirectoryInfo[] assetDirectories = new DirectoryInfo(assetCollector.assetPath).GetDirectories();
                                foreach (DirectoryInfo directory in assetDirectories)
                                {
                                    string[] patterns = assetCollector.searchPatterns.Split(';', ',', '|');
                                    for (int i = 0; i < patterns.Length; i++)
                                    {
                                        FileInfo[] assetFiles = new DirectoryInfo(directory.FullName).GetFiles(patterns[i], SearchOption.AllDirectories);
                                        foreach (FileInfo file in assetFiles)
                                        {
                                            if (file.Extension.Contains("meta"))
                                            {
                                                continue;
                                            }

                                            string relativeAssetName = file.FullName.Substring(Application.dataPath.Length + 1);
                                            string relativeAssetNameWithoutExtension = DEngine.Utility.Path.GetRegularPath(relativeAssetName[..relativeAssetName.LastIndexOf('.')]);

                                            string assetName = Path.Combine("Assets", relativeAssetName);
                                            string assetGUID = AssetDatabase.AssetPathToGUID(assetName);

                                            if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetGUID))
                                            {
                                                ApplyResourceFilter(ref signedAssetBundleList, assetCollector, relativeAssetNameWithoutExtension, assetGUID);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case FilterType.FileOnly:
                            FileInfo assetFile = new FileInfo(assetCollector.assetPath);
                            string assetFileName = assetFile.FullName.Substring(Application.dataPath.Length + 1);
                            string assetNameWithoutExtension = DEngine.Utility.Path.GetRegularPath(assetFileName[..assetFileName.LastIndexOf('.')]);
                            assetFileName = Path.Combine("Assets", assetFileName);
                            string assetFileGUID = AssetDatabase.AssetPathToGUID(assetFileName);

                            if (!m_SourceAssetExceptTypeFilterGUIDArray.Contains(assetFileGUID) && !m_SourceAssetExceptLabelFilterGUIDArray.Contains(assetFileGUID))
                            {
                                ApplyResourceFilter(ref signedAssetBundleList, assetCollector, assetNameWithoutExtension, assetFileGUID);
                            }
                            break;
                    }
                }
            }
        }

        private void ApplyResourceFilter(ref List<string> signedResourceList, AssetCollector resourceRule, string resourceName, string singleAssetGUID = "", string childDirectoryPath = "")
        {
            if (!signedResourceList.Contains(Path.Combine(resourceRule.assetPath, resourceName)))
            {
                signedResourceList.Add(Path.Combine(resourceRule.assetPath, resourceName));

                foreach (DEResource oldResource in GetResources())
                {
                    if (oldResource.Name == resourceName && string.IsNullOrEmpty(oldResource.Variant))
                    {
                        RenameResource(oldResource.Name, oldResource.Variant, resourceName, resourceRule.variant);
                        break;
                    }
                }

                if (!HasResource(resourceName, resourceRule.variant))
                {
                    if (string.IsNullOrEmpty(resourceRule.fileSystem))
                    {
                        resourceRule.fileSystem = null;
                    }

                    AddResource(resourceName, resourceRule.variant, resourceRule.fileSystem, resourceRule.loadType, resourceRule.packed, resourceRule.groups.Split(';', ',', '|'));
                }

                switch (resourceRule.filterType)
                {
                    case FilterType.Root:
                    case FilterType.ChildrenFoldersOnly:
                        string[] patterns = resourceRule.searchPatterns.Split(';', ',', '|');
                        if (childDirectoryPath == "")
                        {
                            childDirectoryPath = resourceRule.assetPath;
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
                                    AssignAsset(assetGUID, resourceName, resourceRule.variant);
                                }
                            }
                        }

                        break;

                    case FilterType.Children:
                    case FilterType.ChildrenFilesOnly:
                    case FilterType.FileOnly:
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