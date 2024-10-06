using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DEngine.Editor;
using Game.DataTable;
using Game.Editor.ResourceTools;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor.DataTableTools
{
    public class DataTableSettingEditorWindows : EditorWindow
    {
        private Vector2 m_ScrollPosition;
        private bool m_FoldoutDataTableGroup = true;
        private bool m_FoldoutLocalizationGroup = true;
        private bool m_FoldoutAssemblyNamesGroup = false;
        private bool m_FoldoutProloadGroup = false;
        private GUIContent m_SaveContent;
        private List<string> m_DataTableName;
        private Dictionary<string, bool> m_DataTableMap;
        private List<KeyValuePair<string, bool>> m_Updates;
        private GameDataTableVersion m_DataTableVersion;

        [MenuItem("DataTable/Setting", priority = 10)]
        private static void OpenWindow()
        {
            var window = GetWindow<DataTableSettingEditorWindows>("DataTable Setting");
            window.minSize = new Vector2(800, 600);
            DataTableSetting.Instance.SaveSetting();
        }

        private void OnEnable()
        {
            m_ScrollPosition = Vector2.zero;
            m_SaveContent = EditorBuiltinIconHelper.GetSave("Save", "");
            m_DataTableName = new List<string>();
            m_DataTableMap = new Dictionary<string, bool>();
            m_Updates = new List<KeyValuePair<string, bool>>();
            string[] ids = AssetDatabase.FindAssets("_table t:textAsset");
            int cnt = ids.Length;
            for (int i = 0; i < cnt; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(ids[i]);
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                if (textAsset)
                {
                    string dataTableName = textAsset.name[..textAsset.name.LastIndexOf("_table", StringComparison.Ordinal)];
                    m_DataTableName.Add(dataTableName);
                }
            }

            var value = AssetDatabase.LoadAssetAtPath<TextAsset>(DataTableSetting.DataTableVersion)?.text;
            if (value != null)
            {
                m_DataTableVersion = JsonConvert.DeserializeObject<GameDataTableVersion>(value);
            }

            m_DataTableVersion ??= new GameDataTableVersion();

            foreach (var data in m_DataTableName)
            {
                m_DataTableMap[data] = m_DataTableVersion.StaticDataTable.Contains(data);
            }
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIAssemblyNames();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIDataTable();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUILocalization();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    DataTableSetting.Instance.SaveSetting();
                }
            }
            EditorGUILayout.EndHorizontal();

            Repaint();
        }

        private void GUIAssemblyNames()
        {
            m_FoldoutAssemblyNamesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutAssemblyNamesGroup, "数据表所用到类型的程序集");
            {
                if (m_FoldoutAssemblyNamesGroup)
                {
                    foreach (var item in DataTableSetting.Instance.AssemblyNames)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly odinEditor = GetWindow<SelectAssembly>();

                            void Save(string[] assemblyNames)
                            {
                                DataTableSetting.Instance.AssemblyNames = assemblyNames.Select(item => item.Replace(".dll", null)).ToArray();
                                DataTableSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(DataTableSetting.Instance.AssemblyNames.Select(item => item.Replace(".dll", null)));

                            odinEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void GUIDataTable()
        {
            m_FoldoutDataTableGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutDataTableGroup, "数据表");
            {
                if (m_FoldoutDataTableGroup)
                {
                    GUIAssetPath("数据表文件导出路径", ref DataTableSetting.Instance.DataTableFolderPath, true);
                    GUIAssetPath("数据表类导出路径", ref DataTableSetting.Instance.CSharpCodePath, true);
                    GUIAssetPath("数据表模板类路径", ref DataTableSetting.Instance.CSharpCodeTemplateFileName);
                    GUIAssetPath("数据表扩展类导出路径", ref DataTableSetting.Instance.ExtensionDirectoryPath, true);
                    GUIOutPath("数据表格路径", ref DataTableSetting.Instance.DataTableExcelsFolder);
                    DataTableSetting.Instance.NameSpace = EditorGUILayout.TextField("数据表命名空间", DataTableSetting.Instance.NameSpace);
                    DataTableSetting.Instance.NameRow = EditorGUILayout.IntField("字段名所在行", DataTableSetting.Instance.NameRow);
                    DataTableSetting.Instance.TypeRow = EditorGUILayout.IntField("类型名所在行", DataTableSetting.Instance.TypeRow);
                    DataTableSetting.Instance.CommentRow = EditorGUILayout.IntField("注释所在行", DataTableSetting.Instance.CommentRow);
                    DataTableSetting.Instance.ContentStartRow = EditorGUILayout.IntField("内容所在行", DataTableSetting.Instance.ContentStartRow);
                    DataTableSetting.Instance.GenerateDataTableEnum = EditorGUILayout.Toggle("是否生成数据表枚举", DataTableSetting.Instance.GenerateDataTableEnum);

                    if (DataTableSetting.Instance.GenerateDataTableEnum)
                    {
                        GUIAssetPath("数据表扩展枚举导出路径", ref DataTableSetting.Instance.DataTableEnumPath, true);
                        EditorGUILayout.HelpBox("注意:数据表第三列将会作为枚举名称,Id 作为枚举值.", MessageType.Warning);
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);


            m_FoldoutProloadGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutProloadGroup, "数据表分类配置");
            {
                if (m_FoldoutProloadGroup)
                {
                    EditorGUILayout.LabelField("数据表", "\t\t\t\t\t是否静态表", EditorStyles.label);

                    m_Updates.Clear();

                    foreach (var data in m_DataTableMap)
                    {
                        bool isStatic = data.Value;
                        GUIPreload(data.Key, ref isStatic);
                        m_Updates.Add(new KeyValuePair<string, bool>(data.Key, isStatic));
                    }

                    foreach (var update in m_Updates)
                    {
                        m_DataTableMap[update.Key] = update.Value;
                    }


                    if (GUILayout.Button(m_SaveContent, GUILayout.Height(20)))
                    {
                        m_DataTableVersion.StaticDataTable.Clear();
                        m_DataTableVersion.DynamicDataTable.Clear();
                        foreach (var data in m_DataTableMap)
                        {
                            if (data.Value)
                            {
                                m_DataTableVersion.StaticDataTable.Add(data.Key);
                            }
                            else
                            {
                                m_DataTableVersion.DynamicDataTable.Add(data.Key);
                            }
                        }
                        
                        GameAssetVersionUitlity.CreateAssetVersion(m_DataTableVersion, DataTableSetting.DataTableVersion);
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void GUILocalization()
        {
            m_FoldoutLocalizationGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutLocalizationGroup, "本地化");
            {
                if (m_FoldoutLocalizationGroup)
                {
                    GUIAssetPath("字典导出路径", ref DataTableSetting.Instance.LocalizationPath);
                    GUIOutPath("字典表格路径", ref DataTableSetting.Instance.LocalizationExcelsFolder);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// 绘制unity 工程内部
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        /// <param name="isFolder"></param>
        private void GUIAssetPath(string header, ref string content, bool isFolder = false)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool valid = !AssetDatabase.LoadAssetAtPath<Object>(content);
                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = Color.yellow;
                content = EditorGUILayout.TextField(header, content, valid ? style : EditorStyles.label);
                Rect rect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(rect, out string path, isFolder))
                {
                    if (!string.Equals(path, content, StringComparison.Ordinal))
                    {
                        content = path;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(content));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制外部路径
        /// </summary>
        /// <param name="header"></param>
        /// <param name="content"></param>
        private void GUIOutPath(string header, ref string content)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(header, content);

                if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
                {
                    string directory = EditorUtility.OpenFolderPanel("Select Output Directory", content, string.Empty);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        if (Directory.Exists(directory) && directory != content)
                        {
                            content = directory;
                        }
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    OpenFolder.Execute(content);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIPreload(string dataTableName, ref bool isStatic)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(dataTableName);
                isStatic = EditorGUILayout.Toggle(isStatic);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}