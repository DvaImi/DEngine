// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System;
using System.IO;
using System.Linq;
using Game.Editor.ResourceTools;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public class GameBuilder : EditorWindow
    {
        private HybridCLRBuilderController m_HybridClrBuilderController;
        private int m_HotfixPlatformIndex;

        private int HotfixPlatformIndex
        {
            get => EditorPrefs.GetInt("BuildPlatform", 2);
            set
            {
                m_HotfixPlatformIndex = value;
                EditorPrefs.SetInt("BuildPlatform", m_HotfixPlatformIndex);
            }
        }

        private bool m_BeginBuild = false;
        private bool m_IsAotGeneric = false;

        [MenuItem("Game/Builder", false, 0)]
        private static void Open()
        {
            GameBuilder window = GetWindow<GameBuilder>("Builder", true);
            window.minSize = new Vector2(800f, 300f);
        }

        private void OnEnable()
        {
            m_HybridClrBuilderController = new HybridCLRBuilderController();
            m_HotfixPlatformIndex = HotfixPlatformIndex;
            m_BeginBuild = false;
        }

        private void OnGUI()
        {
            // Builder
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            GUIItem("(1) 由于ab包依赖裁剪后的dll，在编译hotfix.dl前需要build工程。生成裁剪后的 AOT dll。", "AOT Generic Build", () => m_IsAotGeneric = m_BeginBuild = true, 120);
            int hotfixPlatformIndex = EditorGUILayout.Popup("(2) 选择Hotfix平台。", m_HotfixPlatformIndex, m_HybridClrBuilderController.PlatformNames);
            if (hotfixPlatformIndex != m_HotfixPlatformIndex)
            {
                HotfixPlatformIndex = hotfixPlatformIndex;
            }

            GUIItem("(3) 编译Hotfix.dll。", "Compile", CompileHotfixDll);
            EditorGUILayout.BeginHorizontal();
            GUIResourcesTool();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUIBuildPlayer();
            EditorGUILayout.EndHorizontal();
        }

        private void Update()
        {
            if (m_BeginBuild)
            {
                m_BeginBuild = false;
                BuildPlayer(m_IsAotGeneric);
                m_IsAotGeneric = false;
            }
        }

        private void GUIItem(string content, string button, Action onClick, float width = 100)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(content);
                if (GUILayout.Button(button, GUILayout.Width(width)))
                {
                    onClick?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIResourcesTool()
        {
            EditorGUILayout.LabelField("(4) 编辑Hotfix.dll等资源，并打包。");

            if (GUILayout.Button("Editor", GUILayout.Width(100)))
            {
                EditorWindow window = GetWindow(Type.GetType("Game.Editor.ResourceTools.AssetBundleCollector"));
                window.minSize = new Vector2(1640f, 420f);
            }

            if (GUILayout.Button("Build", GUILayout.Width(130)))
            {
                AssetBundleUtility.StartBuild();
            }
        }

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex);
            CompileDllCommand.CompileDll(buildTarget);
            m_HybridClrBuilderController.CopyDllAssets(buildTarget);
        }

        private void BuildPlayer(bool aot = false)
        {
            BuildReport report = BuildApplicationForPlatform(m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex), aot);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + StringUtility.GetByteLengthString((long)summary.totalSize));

                if (!aot)
                {
                    OpenFolder.OpenFolderPublishAppPath();
                }
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        private void GUIBuildPlayer()
        {
            EditorGUILayout.LabelField($"(5) 构建{m_HybridClrBuilderController.PlatformNames[HotfixPlatformIndex]}工程。");
            if (GUILayout.Button("Edit", GUILayout.Width(100)))
            {
                BuildPlayerWindow.ShowBuildPlayerWindow();
            }

            if (GUILayout.Button("Build", GUILayout.Width(130)))
            {
                m_BeginBuild = true;
            }
        }

        public BuildReport BuildApplicationForPlatform(BuildTarget platform, bool aot)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = aot ? EditorBuildSettings.scenes.Select(x => x.path).ToArray() : new string[] { EditorBuildSettings.scenes[0].path },
                locationPathName = Path.Combine(GameSetting.Instance.PublishAppOutput,
                    m_HybridClrBuilderController.PlatformNames[HotfixPlatformIndex],
                    Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.None
            };

            return BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private string GetFileExtensionForPlatform(BuildTarget platform)
        {
            return platform switch
            {
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.StandaloneOSX => ".app",
                BuildTarget.Android => ".apk",
                BuildTarget.iOS => ".ipa",
                BuildTarget.WebGL => "",
                _ => ".exe",
            };
        }
    }
}