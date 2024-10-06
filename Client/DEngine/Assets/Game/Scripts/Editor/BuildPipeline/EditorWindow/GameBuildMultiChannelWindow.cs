// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class GameBuildMultiChannelWindow : EditorWindow
    {
        private GUIContent m_ExportContent;
        private bool m_Generate;

        [MenuItem("Game/Build Pipeline/MultiChannel", false, 10)]
        private static void Open()
        {
            GameBuildMultiChannelWindow window = GetWindow<GameBuildMultiChannelWindow>("Multi Channel", true);
            window.minSize = new Vector2(800f, 400f);
        }

        private void OnEnable()
        {
            m_ExportContent = EditorGUIUtility.TrTextContentWithIcon("Export", "导出配置", "Project");
            m_Generate = false;
        }

        private void Update()
        {
            if (m_Generate)
            {
                m_Generate = false;
                GameBuildPipeline.GeneratePackingParameterFormExcel();
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal("box");
            {
                GUIMultiChannel();
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(m_ExportContent, GUILayout.Height(30)))
                {
                    m_Generate = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                Repaint();
                GameSetting.Save();
            }
        }

        private void GUIMultiChannel()
        {
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                EditorTools.GUIOutFilePath("多渠道配表格路径", ref GameSetting.Instance.MultiChannelExcelPath, "xlsx");
                GUILayout.Space(5f);
                EditorTools.GUIAssetPath("多渠道配结构导出路径", ref GameSetting.Instance.MultiChannelCodePath, true);
                GUILayout.Space(5f);
                EditorTools.GUIAssetPath("多渠道配表格路径", ref GameSetting.Instance.MultiChannelDataPath, true);
            }
            EditorGUILayout.EndVertical();
        }
    }
}