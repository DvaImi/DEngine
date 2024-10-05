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
using HybridCLR.Editor.Commands;
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
        }

        private void Update()
        {
            if (m_IsAotGeneric)
            {
                m_IsAotGeneric = false;
                StripAOTDllCommand.GenerateStripedAOTDlls();
                AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
            }

            if (m_Compile)
            {
                m_Compile = false;
                GameBuildPipeline.CompileHotfixDll();
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

            if (!Directory.Exists(GameSetting.Instance.AppOutput))
            {
                Directory.CreateDirectory(GameSetting.Instance.AppOutput);
            }

            if (!Directory.Exists(GameSetting.Instance.BundlesOutput))
            {
                Directory.CreateDirectory(GameSetting.Instance.BundlesOutput);
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
                    GUIHyBridCLR();
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
                    GameSetting.Save();
                }
            }
            GUILayout.EndHorizontal();

            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void GUIHyBridCLR()
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
                GameSetting.Instance.HotupdateAssembliesPath = EditorGUILayout.TextField("HotUpdate Dll Path", GameSetting.Instance.HotupdateAssembliesPath);
                Rect hotUpdateRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(hotUpdateRect, out string hotDatePath))
                {
                    if (hotDatePath != null && hotDatePath != GameSetting.Instance.HotupdateAssembliesPath)
                    {
                        GameSetting.Instance.HotupdateAssembliesPath = hotDatePath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.HotupdateAssembliesPath));
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);

            m_FoldoutHotUpdateAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutHotUpdateAssembliesGroup, "HotUpdateAssemblies");
            {
                if (m_FoldoutHotUpdateAssembliesGroup)
                {
                    foreach (var item in GameSetting.Instance.HotUpdateAssemblies)
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
                                GameSetting.Instance.HotUpdateAssemblies = hotUpdateAssemblies.Select(item => item.Replace(".dll", null)).ToArray();
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(GameSetting.Instance.HotUpdateAssemblies.Select(item => item.Replace(".dll", null)));
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
                GameSetting.Instance.PreserveAssembliesPath = EditorGUILayout.TextField("PreserveDll Path", GameSetting.Instance.PreserveAssembliesPath);
                Rect preservePathRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(preservePathRect, out string preservePath))
                {
                    if (preservePath != null && preservePath != GameSetting.Instance.PreserveAssembliesPath)
                    {
                        GameSetting.Instance.PreserveAssembliesPath = preservePath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.PreserveAssembliesPath));
                }
            }
            EditorGUILayout.EndHorizontal();

            m_FoldoutPreserveAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPreserveAssembliesGroup, "PreserveAssemblies");
            {
                if (m_FoldoutPreserveAssembliesGroup)
                {
                    foreach (var item in GameSetting.Instance.PreserveAssemblies)
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
                                GameSetting.Instance.PreserveAssemblies = preserveDll.Select(item => item.Replace(".dll", null)).ToArray();
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(GameSetting.Instance.PreserveAssemblies.Select(item => item.Replace(".dll", null)));
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
                GameSetting.Instance.AOTAssembliesPath = EditorGUILayout.TextField("AOT Dll Path", GameSetting.Instance.AOTAssembliesPath);
                Rect aotPathRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(aotPathRect, out string aotPath))
                {
                    if (aotPath != null && aotPath != GameSetting.Instance.AOTAssembliesPath)
                    {
                        GameSetting.Instance.AOTAssembliesPath = aotPath;
                    }
                }

                if (GUILayout.Button("Go", GUILayout.Width(30)))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(GameSetting.Instance.AOTAssembliesPath));
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            m_FoldoutPatchAOTAssembliesGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutPatchAOTAssembliesGroup, "PatchAOTAssemblies");
            {
                if (m_FoldoutPatchAOTAssembliesGroup)
                {
                    foreach (var item in GameSetting.Instance.AOTAssemblies)
                    {
                        EditorGUILayout.TextField(item);
                    }

                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        if (GUILayout.Button("Editor"))
                        {
                            SelectAssembly assemblyEditor = GetWindow<SelectAssembly>();

                            void Save(string[] aotdll)
                            {
                                GameSetting.Instance.AOTAssemblies = aotdll.Select(item => item.Replace(".dll", null)).ToArray();
                                GameSetting.Instance.SaveSetting();
                                Repaint();
                            }

                            bool WherePredicate(Assembly assembly)
                            {
                                return !assembly.FullName.Contains("Editor");
                            }

                            HashSet<string> hasSelect = new(GameSetting.Instance.AOTAssemblies.Select(item => item.Replace(".dll", null)));
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