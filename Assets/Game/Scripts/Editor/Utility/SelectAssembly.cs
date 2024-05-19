// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-18 23:21:55
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DEngine;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class SelectAssembly : EditorWindow
    {
        private class AssemblyData
        {
            public bool isSelect;
            public string dllName;
            public AssemblyData(bool isSelect, string dllName)
            {
                this.isSelect = isSelect;
                this.dllName = dllName;
            }
        }
        private Vector2 m_ScrollPosition;
        private GUIStyle m_NormalStyle;
        private GUIStyle m_SelectedStyle;
        private HashSet<string> m_HasSelectNames;
        private List<AssemblyData> m_AssemblyDataList;
        private DEngineAction<string[]> m_SaveCallback;
        private Func<Assembly, bool> m_WherePredicate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hasSelectNames">原数据已经选择的</param>
        /// <param name="saveCallBack">保存回调方法</param>
        /// <param name="wherePredicate">筛选回调方法</param>
        public void Open(HashSet<string> hasSelectNames, DEngineAction<string[]> saveCallBack, Func<Assembly, bool> wherePredicate)
        {
            minSize = new Vector2(470, 800);

            m_HasSelectNames = hasSelectNames;
            m_SaveCallback = saveCallBack;
            m_WherePredicate = wherePredicate;

            m_NormalStyle = new GUIStyle();
            m_NormalStyle.normal.textColor = Color.white;

            m_SelectedStyle = new GUIStyle();
            m_SelectedStyle.normal.textColor = Color.green;

            m_AssemblyDataList = new List<AssemblyData>();
            RefreshListData();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                if (m_AssemblyDataList.Count <= 0)
                {
                    EditorGUILayout.HelpBox("未找到程序集,请先Build项目以生成程序集.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("勾选需要添加的dll,然后点击保存生效.", MessageType.Info);
                }
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, true);
                foreach (var item in m_AssemblyDataList)
                {
                    EditorGUILayout.BeginHorizontal();
                    item.isSelect = EditorGUILayout.ToggleLeft(item.dllName, item.isSelect, item.isSelect ? m_SelectedStyle : m_NormalStyle);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("Select All", GUILayout.Width(100)))
                {
                    SelectAll(true);
                }
                if (GUILayout.Button("Cancel All", GUILayout.Width(100)))
                {
                    SelectAll(false);
                }

                if (GUILayout.Button("Reload", GUILayout.Width(120)))
                {
                    RefreshListData();
                }
                if (GUILayout.Button("Save", GUILayout.Width(120)))
                {
                    SaveSelectAssembly();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshListData()
        {
            var projectAssemblyDlls = Utility.Assembly.GetAssemblies();
            m_AssemblyDataList = projectAssemblyDlls.Where(m_WherePredicate).Select(item => new AssemblyData(false, item.GetName().Name)).ToList();
            m_AssemblyDataList.ForEach(data => data.isSelect = m_HasSelectNames.Contains(data.dllName));
            m_AssemblyDataList = m_AssemblyDataList.OrderBy(item => !item.isSelect).ThenBy(item => item.dllName).ToList();
        }

        private void SelectAll(bool value)
        {
            foreach (var item in m_AssemblyDataList)
            {
                item.isSelect = value;
            }
        }

        private string[] GetCurrentSelectedList()
        {
            List<string> result = new List<string>();
            foreach (var item in m_AssemblyDataList)
            {
                if (item.isSelect)
                {
                    result.Add(item.dllName);
                }
            }

            return result.ToArray();
        }

        public void SaveSelectAssembly()
        {
            m_SaveCallback?.Invoke(GetCurrentSelectedList().Select(item => item + ".dll").ToArray());
        }
    }
}