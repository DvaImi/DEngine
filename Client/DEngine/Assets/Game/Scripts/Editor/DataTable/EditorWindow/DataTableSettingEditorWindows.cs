using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public class DataTableSettingEditorWindows : EditorWindow
    {
        private Vector2 m_ScrollPosition;
        private bool m_FoldoutDataTableGroup = true;
        private bool m_FoldoutLocalizationGroup = true;
        private bool m_FoldoutAssemblyNamesGroup = false;
        private GUIContent m_SaveContent;

        [MenuItem("DataTable/Setting", priority = 10)]
        private static void OpenWindow()
        {
            var window = GetWindow<DataTableSettingEditorWindows>("DataTable Setting");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            m_ScrollPosition = Vector2.zero;
            m_SaveContent = EditorBuiltinIconHelper.GetSave("Save", "");
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
                    DataTableSetting.Save();
                    Debug.Log("Save success.");
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
                                DataTableSetting.Save();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return true;
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
                    EditorTools.GUIOutFolderPath("数据表文件导出路径", ref DataTableSetting.Instance.OutputDataTableFolder);
                    EditorTools.GUIAssetPath("数据表类导出路径", ref DataTableSetting.Instance.CSharpCodePath, true);
                    EditorTools.GUIAssetPath("数据表模板类路径", ref DataTableSetting.Instance.CSharpCodeTemplateFileName);
                    EditorTools.GUIAssetPath("数据表扩展类导出路径", ref DataTableSetting.Instance.ExtensionDirectoryPath, true);
                    EditorTools.GUIOutFolderPath("数据表格路径", ref DataTableSetting.Instance.DataTableExcelsFolder);
                    DataTableSetting.Instance.NameSpace = EditorGUILayout.TextField("数据表命名空间", DataTableSetting.Instance.NameSpace);
                    DataTableSetting.Instance.NameRow = EditorGUILayout.IntField("字段名所在行", DataTableSetting.Instance.NameRow);
                    DataTableSetting.Instance.TypeRow = EditorGUILayout.IntField("类型名所在行", DataTableSetting.Instance.TypeRow);
                    DataTableSetting.Instance.CommentRow = EditorGUILayout.IntField("注释所在行", DataTableSetting.Instance.CommentRow);
                    DataTableSetting.Instance.ContentStartRow = EditorGUILayout.IntField("内容所在行", DataTableSetting.Instance.ContentStartRow);
                    DataTableSetting.Instance.GenerateDataTableEnum = EditorGUILayout.Toggle("是否生成数据表枚举", DataTableSetting.Instance.GenerateDataTableEnum);

                    if (DataTableSetting.Instance.GenerateDataTableEnum)
                    {
                        EditorTools.GUIAssetPath("数据表扩展枚举导出路径", ref DataTableSetting.Instance.DataTableEnumPath, true);
                        EditorGUILayout.HelpBox("注意:数据表第三列将会作为枚举名称,Id 作为枚举值.", MessageType.Warning);
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
                    EditorTools.GUIAssetPath("字典导出路径", ref DataTableSetting.Instance.LocalizationPath);
                    EditorTools.GUIOutFolderPath("字典表格路径", ref DataTableSetting.Instance.LocalizationExcelsFolder);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}