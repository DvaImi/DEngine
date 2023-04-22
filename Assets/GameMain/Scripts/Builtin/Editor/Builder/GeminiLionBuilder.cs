// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================
using System;
using UnityEditor;
using UnityEngine;
using HybridCLR.Editor.Commands;
using UnityGameFramework.Editor.ResourceTools;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEditor.SceneManagement;
using System.Linq;

namespace GeminiLion.Editor
{
    public class GeminiLionBuilder : EditorWindow
    {
        private HybridCLRBuilderController m_HybridClrBuilderController;
        private int m_HotfixPlatformIndex;

        private int HotfixPlatformIndex
        {
            get
            {
                return EditorPrefs.GetInt("HybridCLRPlatform", 2);
            }
            set
            {
                m_HotfixPlatformIndex = value;
                EditorPrefs.SetInt("HybridCLRPlatform", m_HotfixPlatformIndex);
            }
        }

        [MenuItem("GeminiLion/Builder", false, 0)]
        private static void Open()
        {
            GeminiLionBuilder window = GetWindow<GeminiLionBuilder>("Builder", true);
            window.minSize = new Vector2(800f, 300f);
        }

        private void OnEnable()
        {
            m_HybridClrBuilderController = new HybridCLRBuilderController();
            m_HotfixPlatformIndex = HotfixPlatformIndex;
        }

        private void OnGUI()
        {
            // Builder
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            GUIItem("(1) 由于ab包依赖裁剪后的dll，在编译hotfix.dl前需要build工程。生成裁剪后的 AOT dll。", "AOT Generic Build", BuildPlayer, 120);
            int hotfixPlatformIndex = EditorGUILayout.Popup("(2) 选择Hotfix平台。", m_HotfixPlatformIndex, m_HybridClrBuilderController.PlatformNames);
            if (hotfixPlatformIndex != m_HotfixPlatformIndex)
            {
                HotfixPlatformIndex = hotfixPlatformIndex;
            }
            GUIItem("(3) 编译Hotfix.dll。", "Compile", CompileHotfixDll);
            EditorGUILayout.BeginHorizontal();
            {
                GUIResourcesTool();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                GUIBuildPlayer();
            }
            EditorGUILayout.EndHorizontal();
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
            if (GUILayout.Button("Edit", GUILayout.Width(100)))
            {
                EditorWindow window = GetWindow(Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceEditor,UnityGameFramework.Editor"));
                window.Show();
            }
            if (GUILayout.Button("BuildAssetBundle", GUILayout.Width(130)))
            {
                Type resType = Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceBuilder,UnityGameFramework.Editor");
                EditorWindow window = GetWindow(resType);
                window.Show();
            }
        }

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex);
            CompileDllCommand.CompileDll(buildTarget);
            m_HybridClrBuilderController.CopyDllAssets(buildTarget);
        }

        private void BuildPlayer()
        {
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = Path.Combine(GeminiLionSetting.Instance.PublishAppOutput, Application.productName + ".exe"),
                target = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex),
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + StringUtility.GetByteLengthString((long)summary.totalSize));
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        private void GUIBuildPlayer()
        {
            EditorGUILayout.LabelField($"(5) 构建{m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex)}工程。");
            if (GUILayout.Button("Edit", GUILayout.Width(100)))
            {
                BuildPlayerWindow.ShowBuildPlayerWindow();
            }
            if (GUILayout.Button("BuildPlayerTarget", GUILayout.Width(130)))
            {
                BuildPlayer();
                OpenFolder.OpenFolderPublishAppPath();
            }
        }
    }
}
