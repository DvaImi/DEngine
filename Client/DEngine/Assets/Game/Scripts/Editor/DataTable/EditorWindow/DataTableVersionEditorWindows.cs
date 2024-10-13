using System;
using System.Collections.Generic;
using Game.DataTable;
using Game.Editor.ResourceTools;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public class DataTableVersionEditorWindows : EditorWindow
    {
        private Vector2 m_ScrollPosition;
        private bool m_FoldoutPreloadGroup = true;
        private GUIContent m_SaveContent;
        private List<string> m_DataTableName;
        private Dictionary<string, bool> m_DataTableMap;
        private List<KeyValuePair<string, bool>> m_Updates;
        private GameDataTableVersion m_DataTableVersion;

        [MenuItem("DataTable/Version", priority = 20)]
        private static void OpenWindow()
        {
            var window = GetWindow<DataTableVersionEditorWindows>("DataTable Version Setting");
            window.minSize = new Vector2(800, 600);
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
                m_DataTableMap[data] = m_DataTableVersion.PreloadDataTable.Contains(data);
            }
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIDataTableVersion();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    m_DataTableVersion.PreloadDataTable.Clear();
                    m_DataTableVersion.DynamicDataTable.Clear();
                    foreach (var data in m_DataTableMap)
                    {
                        if (data.Value)
                        {
                            m_DataTableVersion.PreloadDataTable.Add(data.Key);
                        }
                        else
                        {
                            m_DataTableVersion.DynamicDataTable.Add(data.Key);
                        }
                    }

                    GameAssetVersionUitlity.CreateAssetVersion(m_DataTableVersion, DataTableSetting.DataTableVersion);
                    Debug.Log("Save success.");
                }
            }
            EditorGUILayout.EndHorizontal();

            Repaint();
        }

        private void GUIDataTableVersion()
        {
            EditorGUILayout.Space(10);
            m_FoldoutPreloadGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPreloadGroup, "数据表分类配置");
            {
                if (m_FoldoutPreloadGroup)
                {
                    EditorGUILayout.LabelField("数据表", "\t\t\t\t\t是否预加载", EditorStyles.label);

                    m_Updates.Clear();

                    foreach (var data in m_DataTableMap)
                    {
                        bool isPreload = data.Value;
                        EditorTools.GUIToggle(data.Key, ref isPreload);
                        m_Updates.Add(new KeyValuePair<string, bool>(data.Key, isPreload));
                    }

                    foreach (var update in m_Updates)
                    {
                        m_DataTableMap[update.Key] = update.Value;
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}