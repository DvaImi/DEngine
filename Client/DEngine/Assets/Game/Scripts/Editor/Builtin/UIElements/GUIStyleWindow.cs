using UnityEngine;
using UnityEditor;

namespace Game.Editor
{
    public class GUIStyleWindow : EditorWindow
    {
        private Vector2 scrollVector2 = Vector2.zero;
        private string m_Search = string.Empty;

        [MenuItem("Tools/Show All GUIStyle", false)]
        private static void OpenWindow()
        {
            EditorWindow window = GetWindow(typeof(GUIStyleWindow));
            window.minSize = new Vector2(300, 900);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            m_Search = EditorGUILayout.TextField("", m_Search, "ToolbarSearchTextField");

            if (GUILayout.Button("", "ToolbarSearchCancelButton"))
            {
                m_Search = string.Empty;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            scrollVector2 = GUILayout.BeginScrollView(scrollVector2);

            foreach (GUIStyle style in GUI.skin.customStyles)
            {
                if (m_Search == string.Empty || style.name.Contains(m_Search))
                {
                    DrawStyleItem(style);
                }
            }

            GUILayout.EndScrollView();
        }

        private static void DrawStyleItem(GUIStyle style)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.SelectableLabel(style.name);

            GUILayout.Button("", style);

            EditorGUILayout.EndVertical();
        }
    }
}