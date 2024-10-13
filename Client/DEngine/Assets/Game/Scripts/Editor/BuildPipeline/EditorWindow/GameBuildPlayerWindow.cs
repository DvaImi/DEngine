// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-03-26 16:39:10
// 版 本：1.0
// ========================================================

using System.Collections.Generic;
using System.Linq;
using DEngine.Editor;
using DEngine.Editor.ResourceTools;
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

        [MenuItem("Game/Build Pipeline/Player", false, 0)]
        private static void Open()
        {
            GameBuildPlayerWindow window = GetWindow<GameBuildPlayerWindow>("Build Player", true);
            window.minSize = new Vector2(800f, 400f);
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
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            {
                GUILayout.Space(5f);
                GUIPlatform();
                GUILayout.Space(5f);
                EditorGUILayout.BeginVertical("box");
                {
                    GUIBuildPlayer();
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(5f);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button(m_BuildContent, GUILayout.Height(30)))
                {
                    BuildTarget buildTarget = GameBuildPipeline.GetBuildTarget(DEngineSetting.Instance.BuildPlatform);
                    if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
                    {
                        if (EditorUtility.DisplayDialog("提示", "当前平台与目标平台不符，是否进行切换?", "确认", "取消"))
                        {
                            if (EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget))
                            {
                                m_BeginBuildPlayer = true;
                            }
                        }
                    }
                    else
                    {
                        m_BeginBuildPlayer = true;
                    }
                }

                if (GUILayout.Button(m_SaveContent, GUILayout.Height(30)))
                {
                    GameBuildPipeline.SaveBuildInfo();
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

        private void GUIPlatform()
        {
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Platform", EditorStyles.boldLabel);
                    var hotfixPlatformIndex = (Platform)EditorGUILayout.EnumPopup(DEngineSetting.Instance.BuildPlatform, GUILayout.Width(100));

                    if (!hotfixPlatformIndex.Equals(DEngineSetting.Instance.BuildPlatform))
                    {
                        DEngineSetting.Instance.BuildPlatform = hotfixPlatformIndex;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
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
                DropPathUtility.DropAssetPath("BuildSetting", ref DEngineSetting.Instance.BuildSettingsConfig);

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
                            if (sceneAsset == null)
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

                if (GUILayout.Button("Open", GUILayout.Width(50)))
                {
                    OpenFolder.Execute(DEngineSetting.AppOutput);
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80f)))
                {
                    GameUtility.IO.ClearFolder(DEngineSetting.AppOutput);
                    Debug.Log($"Clear{DEngineSetting.AppOutput} success !");
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}