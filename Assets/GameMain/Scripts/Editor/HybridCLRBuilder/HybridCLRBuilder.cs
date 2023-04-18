// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================
using System;
using UnityEditor;
using UnityEngine;
using HybridCLR.Editor.Commands;
using UnityGameFramework.Editor.ResourceTools;

namespace Dvalmi.Editor
{
    public class HybridCLRBuilder : EditorWindow
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

        [MenuItem("Dvalmi/HybridCLR Builder", false, 0)]
        private static void Open()
        {
            HybridCLRBuilder window = GetWindow<HybridCLRBuilder>("HybridCLR Builder", true);
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
            EditorGUILayout.BeginVertical("box");
            {
                GUIItem("由于ab包依赖裁剪后的dll，在编译hotfix.dl前需要build工程。", "Build", BuildPlayerWindow.ShowBuildPlayerWindow);
                int hotfixPlatformIndex = EditorGUILayout.Popup("选择hotfix平台。", m_HotfixPlatformIndex, m_HybridClrBuilderController.PlatformNames);
                if (hotfixPlatformIndex != m_HotfixPlatformIndex)
                {
                    HotfixPlatformIndex = hotfixPlatformIndex;
                }
                GUIItem("编译hotfix.dll。", "Compile", CompileHotfixDll);
                GUIResourcesTool();
            }
            EditorGUILayout.EndVertical();
        }

        private void GUIItem(string content, string button, Action onClick)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(content);
                if (GUILayout.Button(button, GUILayout.Width(100)))
                {
                    onClick?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIResourcesTool()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("编辑hotfix.dll等资源，并打包。");
                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                    EditorWindow window = GetWindow(Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceEditor,UnityGameFramework.Editor"));
                    window.Show();
                }
                if (GUILayout.Button("Build", GUILayout.Width(100)))
                {
                    Type resType = Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceBuilder,UnityGameFramework.Editor");
                    EditorWindow window = GetWindow(resType);
                    window.Show();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex);
            CompileDllCommand.CompileDll(buildTarget);
            m_HybridClrBuilderController.CopyDllAssets(buildTarget);
        }
    }
}
