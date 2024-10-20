// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class GameBuildPlayerWindow : EditorWindow
    {
        private bool m_BeginBuildPlayer = false;
        private bool m_FoldoutBuildSceneGroup = true;
        private Vector2 m_ScrollPosition;
        private List<string> m_DefaultSceneNames;
        private GUIContent m_BuildContent;
        private GUIContent m_SaveContent;
        private const string NoneOptionName = "<None>";
        private int m_BuildEventHandlerTypeNameIndex;
        private List<string> m_BuildEventHandlerTypeNames;

        [MenuItem("Game/Build Pipeline/Player", false, 0)]
        private static void Open()
        {
            GameBuildPlayerWindow window = GetWindow<GameBuildPlayerWindow>("Build Player", true);
            window.minSize = new Vector2(800f, 400f);
            window.Show();
        }

        private void Update()
        {
            if (m_BeginBuildPlayer)
            {
                m_BeginBuildPlayer = false;
                GameBuildPipeline.BuildPlayer();
            }
        }

        private void OnEnable()
        {
            m_BeginBuildPlayer = false;
            m_ScrollPosition = Vector2.zero;
            m_DefaultSceneNames = DEngineSetting.Instance.DefaultSceneNames.ToList();
            m_DefaultSceneNames ??= new List<string>();
            m_BuildContent = EditorBuiltinIconHelper.GetPlatformIconContent("Build", "构建当前平台应用");
            m_SaveContent = EditorBuiltinIconHelper.GetSave("Save", "");


            m_BuildEventHandlerTypeNameIndex = 0;
            m_BuildEventHandlerTypeNames = new List<string>
            {
                NoneOptionName
            };

            m_BuildEventHandlerTypeNames.AddRange(GameType.GetRuntimeOrEditorTypeNames(typeof(IBuildPlayerEventHandler)));
            for (int i = 0; i < m_BuildEventHandlerTypeNames.Count; i++)
            {
                if (DEngineSetting.Instance.BuildPlayerEventHandlerTypeName == m_BuildEventHandlerTypeNames[i])
                {
                    m_BuildEventHandlerTypeNameIndex = i;
                    break;
                }
            }
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(10f);
                GameBuildPipeline.GUIPlatform();
                GUILayout.Space(10f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIBuildPlayer();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Build Event Handler", GUILayout.Width(160f));
                string[] names = m_BuildEventHandlerTypeNames.ToArray();
                int selectedIndex = EditorGUILayout.Popup(m_BuildEventHandlerTypeNameIndex, names);
                if (selectedIndex != m_BuildEventHandlerTypeNameIndex)
                {
                    m_BuildEventHandlerTypeNameIndex = selectedIndex;
                    DEngineSetting.Instance.BuildPlayerEventHandlerTypeName = selectedIndex <= 0 ? string.Empty : names[selectedIndex];
                    DEngineSetting.Save();
                    Debug.Log("Set build event handler success.");
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button(m_BuildContent, GUILayout.Height(30)))
                {
                    m_BeginBuildPlayer = true;
                }

                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    GameBuildPipeline.SaveBuildSetting();
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

        private void GUIBuildPlayer()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("BuildPlayer", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            {
                DropPathUtility.DropAndPingAssetPath("BuildSetting", ref DEngineSetting.Instance.BuildSettingsConfig);

                bool changed = false;
                GUILayout.Space(5f);
                m_FoldoutBuildSceneGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutBuildSceneGroup, "Build Scenes");
                Rect defaultSceneRect = GUILayoutUtility.GetLastRect();
                if (DropPathUtility.DropPath(defaultSceneRect, out string sceneAssetPath))
                {
                    SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneAssetPath);
                    if (sceneAsset)
                    {
                        if (!m_DefaultSceneNames.Contains(sceneAssetPath))
                        {
                            m_DefaultSceneNames.Add(sceneAssetPath);
                            changed = true;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("拖入场景文件");
                    }
                }

                if (m_FoldoutBuildSceneGroup)
                {
                    for (int i = 0; i < m_DefaultSceneNames.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(m_DefaultSceneNames[i]);
                            if (!sceneAsset)
                            {
                                continue;
                            }

                            EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);

                            if (GUILayout.Button("X", GUILayout.Width(30)))
                            {
                                m_DefaultSceneNames.RemoveAt(i);
                                changed = true;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (changed)
                {
                    DEngineSetting.Instance.DefaultSceneNames = m_DefaultSceneNames.ToArray();
                    DEngineSetting.Save();
                    GameBuildPipeline.SaveBuildSetting();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
                GUILayout.Space(5f);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("AppOutput", EditorStyles.boldLabel);
                GUI.enabled = false;
                EditorGUILayout.LabelField(DEngineSetting.AppOutput);
                GUI.enabled = true;

                if (GUILayout.Button("Reveal", GUILayout.Width(80)))
                {
                    EditorUtility.RevealInFinder(DEngineSetting.AppOutput);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80)))
                {
                    GameUtility.IO.ClearFolder(DEngineSetting.AppOutput);
                    Debug.Log($"Clear{DEngineSetting.AppOutput} success !");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}