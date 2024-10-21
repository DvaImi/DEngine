// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class GameBuildHybridCLRWindow : EditorWindow
    {
        private bool m_EnableHybridCLR = false;
        private bool m_FoldoutHotUpdateAssembliesGroup = true;
        private bool m_FoldoutPatchAOTAssembliesGroup = true;
        private bool m_FoldoutPreserveAssembliesGroup = true;
        private bool m_IsAotGeneric = false;
        private bool m_Compile = false;
        private Vector2 m_ScrollPosition;
        private GUIContent m_BuildContent;
        private GUIContent m_CompileContent;
        private GUIContent m_SaveContent;

        [MenuItem("Game/Build Pipeline/HybridCLR", false, 1)]
        private static void Open()
        {
            GameBuildHybridCLRWindow window = GetWindow<GameBuildHybridCLRWindow>("Build HybridCLR", true);
            window.minSize = new Vector2(900f, 600f);
            window.Show();
        }

        private void Update()
        {
            if (m_IsAotGeneric)
            {
                m_IsAotGeneric = false;
                GameBuildPipeline.GenerateStripedAOT();
            }

            if (m_Compile)
            {
                m_Compile = false;
                GameBuildPipeline.CompileUpdateDll();
            }
        }

        private void OnEnable()
        {
            m_IsAotGeneric = false;
            m_EnableHybridCLR = SettingsUtil.Enable;
            m_ScrollPosition = Vector2.zero;
            m_BuildContent = EditorBuiltinIconHelper.GetPlatformIconContent("AOT Generic", "生成AOT");
            m_CompileContent = EditorGUIUtility.TrTextContentWithIcon("Compile", "编译热更代码", "Assembly Icon");
            m_SaveContent = EditorBuiltinIconHelper.GetSave("Save", "保存配置");

            if (!Directory.Exists(DEngineSetting.AppOutput))
            {
                Directory.CreateDirectory(DEngineSetting.AppOutput);
            }

            if (!Directory.Exists(DEngineSetting.BundlesOutput))
            {
                Directory.CreateDirectory(DEngineSetting.BundlesOutput);
            }

            GameBuildPipeline.RefreshPackages();
            GameBuildPipeline.CheckEnableHybridCLR();
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(10f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIHybridCLR();
                }
                GUILayout.Space(5f);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_BuildContent, GUILayout.Height(30)))
                {
                    m_IsAotGeneric = true;
                }

                if (GUILayout.Button(m_CompileContent, GUILayout.Height(30)))
                {
                    m_Compile = true;
                }

                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    GameBuildPipeline.SaveHybridCLR();
                    DEngineSetting.Save();
                    Debug.Log("Save success.");
                }
            }
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void GUIHybridCLR()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("HybridCLR", EditorStyles.boldLabel);
                bool enableHybridCLR = EditorGUILayout.ToggleLeft("EnableHybridCLR", m_EnableHybridCLR);
                if (m_EnableHybridCLR != enableHybridCLR)
                {
                    if (EditorUtility.DisplayDialog("HybridCLR", $"{(enableHybridCLR ? "开启" : "关闭")} HybridCLR时，请{(enableHybridCLR ? "激活" : "关闭")}资源收集器有关HybridCLR的资源", "确定", "取消"))
                    {
                        m_EnableHybridCLR = enableHybridCLR;
                        if (m_EnableHybridCLR)
                        {
                            GameBuildPipeline.EnableHybridCLR();
                        }
                        else
                        {
                            GameBuildPipeline.DisableHybridCLR();
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal();
            {
                DEngineSetting.Instance.UpdateAssembliesPath = EditorGUILayout.TextField("Update Assemblies Path", DEngineSetting.Instance.UpdateAssembliesPath);
                Rect hotUpdateRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(hotUpdateRect, out string hotDatePath))
                {
                    if (hotDatePath != null && hotDatePath != DEngineSetting.Instance.UpdateAssembliesPath)
                    {
                        DEngineSetting.Instance.UpdateAssembliesPath = hotDatePath;
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Width(80)))
                {
                    EditorUtility.RevealInFinder(DEngineSetting.Instance.UpdateAssembliesPath);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            m_FoldoutHotUpdateAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutHotUpdateAssembliesGroup, "UpdateAssembliesPath");
            {
                if (m_FoldoutHotUpdateAssembliesGroup)
                {
                    foreach (var item in DEngineSetting.Instance.UpdateAssemblies)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] hotUpdateAssemblies)
                            {
                                DEngineSetting.Instance.UpdateAssemblies = hotUpdateAssemblies.Select(item => item.Replace(".dll", null)).ToArray();
                                DEngineSetting.Save();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(DEngineSetting.Instance.UpdateAssemblies.Select(item => item.Replace(".dll", null)));
                            assemblyEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                DEngineSetting.Instance.PreserveAssembliesPath = EditorGUILayout.TextField("PreserveDll Path", DEngineSetting.Instance.PreserveAssembliesPath);
                Rect preservePathRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(preservePathRect, out string preservePath))
                {
                    if (preservePath != null && preservePath != DEngineSetting.Instance.PreserveAssembliesPath)
                    {
                        DEngineSetting.Instance.PreserveAssembliesPath = preservePath;
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Width(80)))
                {
                    EditorUtility.RevealInFinder(DEngineSetting.Instance.PreserveAssembliesPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            m_FoldoutPreserveAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPreserveAssembliesGroup, "PreserveAssemblies");
            {
                if (m_FoldoutPreserveAssembliesGroup)
                {
                    foreach (var item in DEngineSetting.Instance.PreserveAssemblies)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] preserveDll)
                            {
                                DEngineSetting.Instance.PreserveAssemblies = preserveDll.Select(item => item.Replace(".dll", null)).ToArray();
                                DEngineSetting.Save();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(DEngineSetting.Instance.PreserveAssemblies.Select(item => item.Replace(".dll", null)));
                            assemblyEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                DEngineSetting.Instance.AOTAssembliesPath = EditorGUILayout.TextField("AOT Dll Path", DEngineSetting.Instance.AOTAssembliesPath);
                Rect aotPathRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(aotPathRect, out string aotPath))
                {
                    if (aotPath != null && aotPath != DEngineSetting.Instance.AOTAssembliesPath)
                    {
                        DEngineSetting.Instance.AOTAssembliesPath = aotPath;
                    }
                }

                if (GUILayout.Button("Reveal", GUILayout.Width(80), GUILayout.Width(80)))
                {
                    EditorUtility.RevealInFinder(DEngineSetting.Instance.AOTAssembliesPath);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            m_FoldoutPatchAOTAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPatchAOTAssembliesGroup, "PatchAOTAssemblies");
            {
                if (m_FoldoutPatchAOTAssembliesGroup)
                {
                    foreach (var item in DEngineSetting.Instance.AOTAssemblies)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] dlls)
                            {
                                DEngineSetting.Instance.AOTAssemblies = dlls.Select(item => item.Replace(".dll", null)).ToArray();
                                DEngineSetting.Save();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(DEngineSetting.Instance.AOTAssemblies.Select(item => item.Replace(".dll", null)));
                            assemblyEditor.Open(hasSelect, Save, WherePredicate);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}