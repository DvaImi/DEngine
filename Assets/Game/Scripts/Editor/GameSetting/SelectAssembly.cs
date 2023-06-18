// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-18 23:21:55
// 版 本：1.0
// ========================================================
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DEngine;
using Game.Editor.Builder;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class SelectAssembly : OdinEditorWindow
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
        private List<AssemblyData> m_AssemblyDataList;
        private GUIStyle m_NormalStyle;
        private GUIStyle m_SelectedStyle;
        public void Open()
        {
            var window = GetWindow<SelectAssembly>("Select AOT Assembly");
            window.minSize = new Vector2(470, 800);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_NormalStyle = new GUIStyle();
            m_NormalStyle.normal.textColor = Color.white;

            m_SelectedStyle = new GUIStyle();
            m_SelectedStyle.normal.textColor = Color.green;

            m_AssemblyDataList = new List<AssemblyData>();
            RefreshListData();
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (m_AssemblyDataList.Count <= 0)
            {
                EditorGUILayout.HelpBox("未找到程序集,请先Build项目以生成程序集.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("勾选需要添加到AOT元数据补充的dll,然后点击保存生效.", MessageType.Info);
            }
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, true);
            for (int i = 0; i < m_AssemblyDataList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var item = m_AssemblyDataList[i];
                item.isSelect = EditorGUILayout.ToggleLeft(item.dllName, item.isSelect, item.isSelect ? m_SelectedStyle : m_NormalStyle);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
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
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }

        private void RefreshListData()
        {
            var projectAssemblyDlls = Utility.Assembly.GetAssemblies();

            m_AssemblyDataList = projectAssemblyDlls.Where(item => !item.FullName.Contains("Editor")).Select(item => new AssemblyData(false, item.GetName().Name)).ToList();

            HashSet<string> aotDllNames = new(GameSetting.Instance.AOTDllNames.Select(item => item.Replace(".dll", null)));

            m_AssemblyDataList.ForEach(data => data.isSelect = aotDllNames.Contains(data.dllName));

            m_AssemblyDataList = m_AssemblyDataList.OrderByDescending(data => data.isSelect).ToList();
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
            GameSetting.Instance.AOTDllNames = GetCurrentSelectedList().Select(item => item + ".dll").ToArray();
            GameSetting.Instance.SaveSetting();
            if (HasOpenInstances<GameBuilderWindow>())
            {
                GetWindow<GameBuilderWindow>().Repaint();
            }
        }
    }
}