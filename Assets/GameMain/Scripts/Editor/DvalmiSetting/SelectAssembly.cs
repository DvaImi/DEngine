// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-18 23:21:55
// 版 本：1.0
// ========================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameFramework;
using NUnit.Framework.Interfaces;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Dvalmi.Editor
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
        private Vector2 scrollPosition;
        private string[] selectedDllList;
        private List<AssemblyData> assemblyDataList;
        private GUIStyle normalStyle;
        private GUIStyle selectedStyle;
        public void Open()
        {
            var window = GetWindow<SelectAssembly>("Select AOT Assembly");
            window.minSize = new Vector2(300, 600);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            normalStyle = new GUIStyle();
            normalStyle.normal.textColor = Color.white;

            selectedStyle = new GUIStyle();
            selectedStyle.normal.textColor = Color.green;

            assemblyDataList = new List<AssemblyData>();
            RefreshListData();
        }

        protected override void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (assemblyDataList.Count <= 0)
            {
                EditorGUILayout.HelpBox("未找到程序集,请先Build项目以生成程序集.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("勾选需要添加到AOT元数据补充的dll,然后点击保存生效.", MessageType.Info);
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
            for (int i = 0; i < assemblyDataList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var item = assemblyDataList[i];
                item.isSelect = EditorGUILayout.ToggleLeft(item.dllName, item.isSelect, item.isSelect ? selectedStyle : normalStyle);
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
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reload", GUILayout.Width(120)))
            {
                RefreshListData();
            }
            if (GUILayout.Button("Save", GUILayout.Width(120)))
            {
                SaveAotDllAssembly();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }

        private void SaveAotDllAssembly()
        {
            DvalimiSettingEditorWindows windows = GetWindow<DvalimiSettingEditorWindows>();
            windows.AOTDllNames= GetCurrentSelectedList().Select(item => item + ".dll").ToArray();
            windows.SaveSetting();
        }

        private void RefreshListData()
        {
            selectedDllList = GetCurrentSelectedList();

            var projectAssemblyDlls = GetProjectAssemblyDlls();

            assemblyDataList = projectAssemblyDlls
                .Select(item => new AssemblyData(!item.GetName().Name.Contains("Editor") && item.GetName().Name != "Game", item.GetName().Name))
                .ToList();

            HashSet<string> aotDllNames = new HashSet<string>(DvalmiSetting.Instance.AOTDllNames.Select(item => item.Replace(".dll", null)));

            assemblyDataList.ForEach(data => data.isSelect = aotDllNames.Contains(data.dllName));

            assemblyDataList = assemblyDataList.OrderByDescending(data => data.isSelect).ToList();
        }

        private void SelectAll(bool value)
        {
            foreach (var item in assemblyDataList)
            {
                item.isSelect = value;
            }
        }

        private string[] GetCurrentSelectedList()
        {
            List<string> result = new List<string>();
            foreach (var item in assemblyDataList)
            {
                if (item.isSelect)
                {
                    result.Add(item.dllName);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取AOT 程序集
        /// </summary>
        /// <returns></returns>
        private Assembly[] GetProjectAssemblyDlls()
        {
            return Utility.Assembly.GetAssemblies();
        }
    }
}